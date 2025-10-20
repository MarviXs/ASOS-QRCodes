<template>
  <PageLayout :breadcrumbs="breadcrumbs">
    <template #actions>
      <q-select
        ref="dateSelectRef"
        class="shadow date-picker"
        v-model="dateRangeSelected"
        :options="dateRanges"
        :label="t('analytics.date_range')"
        option-value="dateRange"
        option-label="name"
        filled
        dense
        bg-color="white"
        emit-value
        hide-bottom-space
        map-options
        @update:model-value="onDateRangeChange"
      >
        <template #prepend>
          <q-icon :name="mdiCalendarRange" />
        </template>
        <template #after-options>
          <q-item clickable>
            <q-item-section>
              <q-item-label>{{ t('analytics.custom') }}</q-item-label>
            </q-item-section>
            <q-popup-proxy transition-show="scale" transition-hide="scale" @before-show="prepareCustomRange">
              <q-date v-model="customRangeTemp" range mask="YYYY/MM/DD" color="primary">
                <div class="row items-center justify-end q-gutter-sm q-pa-sm">
                  <q-btn
                    color="primary"
                    flat
                    dense
                    :label="t('analytics.apply_range')"
                    v-close-popup
                    @click="applyCustomRange"
                  />
                </div>
              </q-date>
            </q-popup-proxy>
          </q-item>
        </template>
        <template #selected-item="scope">
          <template v-if="scope.opt?.name">{{ scope.opt.name }}</template>
          <template v-else-if="dateRangeSelected"> {{ dateRangeSelected.from }} - {{ dateRangeSelected.to }} </template>
        </template>
      </q-select>
    </template>
    <q-table
      v-model:pagination="pagination"
      :rows="scanRecords"
      :columns="columns"
      :loading="loading"
      flat
      binary-state-sort
      :rows-per-page-options="[10, 20, 50]"
      class="shadow"
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      @request="(requestProp) => fetchScanRecords(requestProp.pagination)"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiChartLine" class="q-mb-md" size="50px"></q-icon>
          {{ message }}
        </div>
      </template>
    </q-table>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { mdiCalendarRange, mdiChartLine } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import QRCodeService from '@/api/services/QRCodeService';
import ScanRecordService, {
  type ScanRecordsQueryParams,
  type ScanRecordsResponse,
} from '@/api/services/ScanRecordService';
import type { QTableProps } from 'quasar';

type PresetKey = 'last7' | 'last14' | 'last30' | 'last90';
type DatePickerRange = { from: string; to: string };
interface DateRangeOption {
  key: PresetKey;
  name: string;
  dateRange: DatePickerRange;
}
interface DateRange {
  start: Date;
  end: Date;
}

const route = useRoute();
const { t, locale } = useI18n();

const qrCodeId = ref<string>(route.params.id as string);
const qrCodeName = ref<string>('');

const pagination = ref<PaginationClient>({
  sortBy: 'createdAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});

const loading = ref(false);
const scanRecordsPaginated = ref<ScanRecordsResponse>();
const scanRecords = computed(() => scanRecordsPaginated.value?.items ?? []);

const dateSelectRef = ref();
const dateRanges = ref<DateRangeOption[]>([]);
const activePresetKey = ref<PresetKey | 'custom'>('last30');
const dateRangeSelected = ref<DatePickerRange>(createPickerRange(createRangeFromPreset(30)));
const customRangeTemp = ref<DatePickerRange | null>(null);
const dateRange = ref<DateRange>(createRangeFromPreset(30));
let suppressNextUpdate = false;
const presetDefinitions: Array<{ key: PresetKey; days: number; labelKey: string }> = [
  { key: 'last7', days: 7, labelKey: 'analytics.last_7_days' },
  { key: 'last14', days: 14, labelKey: 'analytics.last_14_days' },
  { key: 'last30', days: 30, labelKey: 'analytics.last_30_days' },
  { key: 'last90', days: 90, labelKey: 'analytics.last_90_days' },
];

const breadcrumbs = computed(() => [
  { label: 'QR Codes', to: '/qr-codes' },
  { label: qrCodeName.value || t('analytics.title') },
]);

const dateRangeIso = computed(() => ({
  start: dateRange.value ? dateRange.value.start.toISOString() : undefined,
  end: dateRange.value ? dateRange.value.end.toISOString() : undefined,
}));

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'browserInfo',
    label: t('analytics.browser'),
    field: 'browserInfo',
    align: 'left',
    sortable: true,
  },
  {
    name: 'operatingSystem',
    label: t('analytics.operating_system'),
    field: 'operatingSystem',
    align: 'left',
    sortable: true,
  },
  {
    name: 'deviceType',
    label: t('analytics.device_type'),
    field: 'deviceType',
    align: 'left',
    sortable: true,
  },
  {
    name: 'country',
    label: t('analytics.country'),
    field: 'country',
    align: 'left',
    sortable: true,
  },
  {
    name: 'createdAt',
    label: t('analytics.scan_time'),
    field: 'createdAt',
    align: 'right',
    sortable: true,
    format(val) {
      return new Date(val).toLocaleString(locale.value);
    },
  },
]);

async function fetchQRCodeName(currentId: string) {
  const { data, error } = await QRCodeService.getQRCode(currentId);
  if (error) {
    handleError(error, 'Failed to load QR code');
    return;
  }
  qrCodeName.value = data?.displayName ?? '';
}

async function fetchScanRecords(paginationTable: PaginationTable) {
  if (!qrCodeId.value) {
    return;
  }

  const query: ScanRecordsQueryParams = {
    QRCodeId: qrCodeId.value,
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
    StartDate: dateRangeIso.value.start,
    EndDate: dateRangeIso.value.end,
  };

  loading.value = true;
  const { data, error } = await ScanRecordService.getScanRecords(query);
  loading.value = false;

  if (error) {
    handleError(error, 'Failed to load scan records');
    return;
  }

  scanRecordsPaginated.value = data;
  if (data) {
    pagination.value.sortBy = paginationTable.sortBy ?? pagination.value.sortBy;
    pagination.value.descending = paginationTable.descending ?? pagination.value.descending;
    pagination.value.page = data.currentPage ?? paginationTable.page;
    pagination.value.rowsPerPage = data.pageSize ?? paginationTable.rowsPerPage;
    pagination.value.rowsNumber = data.totalCount ?? 0;
  }
}

function createRangeFromPreset(days: number): DateRange {
  const end = endOfDay(new Date());
  const start = startOfDay(new Date(end));
  start.setDate(start.getDate() - (days - 1));
  return { start, end };
}

function createPickerRange(range: DateRange): DatePickerRange {
  return {
    from: formatDateLabel(range.start),
    to: formatDateLabel(range.end),
  };
}

function startOfDay(date: Date): Date {
  const day = new Date(date);
  day.setHours(0, 0, 0, 0);
  return day;
}

function endOfDay(date: Date): Date {
  const day = new Date(date);
  day.setHours(23, 59, 59, 999);
  return day;
}

function formatDateLabel(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}/${month}/${day}`;
}

function parsePickerDate(value: string): Date {
  const [year, month, day] = value.split('/').map(Number);
  return new Date(year, (month ?? 1) - 1, day ?? 1);
}

function refreshWithRange(newRange: DateRange, options?: { fetch?: boolean; resetPage?: boolean }) {
  const { fetch = true, resetPage = true } = options ?? {};
  dateRange.value = newRange;
  if (resetPage) {
    pagination.value.page = 1;
  }
  if (fetch) {
    const paginationTable: PaginationTable = {
      sortBy: pagination.value.sortBy,
      descending: pagination.value.descending,
      page: pagination.value.page,
      rowsPerPage: pagination.value.rowsPerPage,
    };
    fetchScanRecords(paginationTable);
  }
}

function selectionToDateRange(selection: DatePickerRange): DateRange {
  const start = startOfDay(parsePickerDate(selection.from));
  const end = endOfDay(parsePickerDate(selection.to));
  return { start, end };
}

function normalizePickerRange(range: DatePickerRange): DatePickerRange {
  const startDateRaw = parsePickerDate(range.from);
  const endDateRaw = parsePickerDate(range.to);
  const [minDate, maxDate] = startDateRaw <= endDateRaw ? [startDateRaw, endDateRaw] : [endDateRaw, startDateRaw];
  const normalizedStart = startOfDay(minDate);
  const normalizedEnd = endOfDay(maxDate);
  return {
    from: formatDateLabel(normalizedStart),
    to: formatDateLabel(normalizedEnd),
  };
}

function applySelectionRange(selection: DatePickerRange, options?: { fetch?: boolean; resetPage?: boolean }) {
  if (!selection?.from || !selection?.to) {
    return;
  }
  const newRange = selectionToDateRange(selection);
  refreshWithRange(newRange, options);
}

function cloneRange(range: DatePickerRange): DatePickerRange {
  return { from: range.from, to: range.to };
}

function scheduleSuppressionReset() {
  nextTick(() => {
    suppressNextUpdate = false;
  });
}

function setSelectedRange(range: DatePickerRange, options?: { suppressIfChanged?: boolean }) {
  if (!range) {
    return;
  }
  const suppress = options?.suppressIfChanged ?? false;
  const current = dateRangeSelected.value;
  const sameReference = current === range;
  const sameValue = current ? rangesEqual(current, range) : false;

  if (sameReference) {
    return;
  }

  if (suppress) {
    suppressNextUpdate = !sameValue;
    if (suppressNextUpdate) {
      scheduleSuppressionReset();
    }
  }

  dateRangeSelected.value = range;
}

function refreshPresetRanges() {
  const presets = presetDefinitions.map<DateRangeOption>((preset) => ({
    key: preset.key,
    name: t(preset.labelKey),
    dateRange: createPickerRange(createRangeFromPreset(preset.days)),
  }));
  dateRanges.value = presets;

  if (activePresetKey.value !== 'custom') {
    const activePreset = presets.find((preset) => preset.key === activePresetKey.value);
    if (activePreset) {
      setSelectedRange(activePreset.dateRange, { suppressIfChanged: true });
    }
  }
}

function rangesEqual(left: DatePickerRange, right: DatePickerRange): boolean {
  return left.from === right.from && left.to === right.to;
}

function onDateRangeChange(selection: DatePickerRange | null) {
  if (suppressNextUpdate) {
    suppressNextUpdate = false;
    return;
  }
  if (!selection?.from || !selection?.to) {
    return;
  }
  const presetMatch = dateRanges.value.find((option) =>
    rangesEqual(option.dateRange, selection),
  );
  activePresetKey.value = presetMatch?.key ?? 'custom';
  const effectiveSelection = presetMatch?.dateRange ?? selection;
  setSelectedRange(effectiveSelection);
  applySelectionRange(effectiveSelection);
}

function prepareCustomRange() {
  customRangeTemp.value = dateRangeSelected.value
    ? cloneRange(dateRangeSelected.value)
    : createPickerRange(createRangeFromPreset(30));
}

function applyCustomRange() {
  if (!customRangeTemp.value?.from || !customRangeTemp.value?.to) {
    return;
  }
  const normalized = normalizePickerRange(customRangeTemp.value);
  setSelectedRange(normalized, { suppressIfChanged: true });
  activePresetKey.value = 'custom';
  applySelectionRange(normalized);
  dateSelectRef.value?.hidePopup();
}

refreshPresetRanges();
applySelectionRange(dateRangeSelected.value, { fetch: false, resetPage: false });

watch(
  () => locale.value,
  () => {
    const previousSelection =
      activePresetKey.value === 'custom' && dateRangeSelected.value
        ? cloneRange(dateRangeSelected.value)
        : undefined;
    refreshPresetRanges();
    if (activePresetKey.value === 'custom' && previousSelection) {
      setSelectedRange(previousSelection, { suppressIfChanged: true });
    }
    applySelectionRange(dateRangeSelected.value, { fetch: false, resetPage: false });
  },
);

watch(
  () => route.params.id,
  (newId) => {
    if (typeof newId === 'string') {
      qrCodeId.value = newId;
      activePresetKey.value = 'last30';
      refreshPresetRanges();
      const defaultPreset = dateRanges.value.find((preset) => preset.key === 'last30');
      const selection = defaultPreset?.dateRange ?? dateRangeSelected.value;
      if (defaultPreset) {
        setSelectedRange(defaultPreset.dateRange, { suppressIfChanged: true });
      }
      applySelectionRange(selection, { fetch: true });
      fetchQRCodeName(newId);
    }
  },
  { immediate: true },
);
</script>
