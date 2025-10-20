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

    <div class="analytics-content">
      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-6">
          <q-card class="shadow metric-card">
            <q-card-section class="metric-card__section">
              <div class="metric-card__label text-caption text-uppercase text-secondary text-weight-medium">
                {{ t('analytics.scans_over_period') }}
              </div>
              <div class="metric-card__value text-h4 text-primary text-weight-bold">
                <template v-if="!analyticsLoading">{{ formattedPeriodScans }}</template>
                <q-skeleton v-else type="text" width="70px" />
              </div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-md-6">
          <q-card class="shadow metric-card">
            <q-card-section class="metric-card__section">
              <div class="metric-card__label text-caption text-uppercase text-secondary text-weight-medium">
                {{ t('analytics.lifetime_scans') }}
              </div>
              <div class="metric-card__value text-h4 text-primary text-weight-bold">
                <template v-if="!analyticsLoading">{{ formattedLifetimeScans }}</template>
                <q-skeleton v-else type="text" width="70px" />
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <q-card class="shadow chart-card">
        <q-card-section>
          <div class="chart-title text-subtitle1 text-secondary text-weight-medium">
            {{ t('analytics.daily_scans_chart') }}
          </div>
          <div class="chart-container">
            <template v-if="hasDailyData">
              <apexchart type="line" height="320" :series="dailyLineSeries" :options="dailyLineOptions" />
            </template>
            <div v-else class="chart-empty text-secondary text-weight-medium">
              {{ t('analytics.no_chart_data') }}
            </div>
          </div>
        </q-card-section>
        <q-inner-loading :showing="analyticsLoading">
          <q-spinner color="primary" />
        </q-inner-loading>
      </q-card>

      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-4">
          <q-card class="shadow chart-card">
            <q-card-section>
              <div class="chart-title text-subtitle1 text-secondary text-weight-medium">
                {{ t('analytics.operating_systems_chart') }}
              </div>
              <div class="chart-container">
                <template v-if="hasOperatingSystemData">
                  <apexchart type="donut" height="320" :series="operatingSystemSeries" :options="operatingSystemOptions" />
                </template>
                <div v-else class="chart-empty text-secondary text-weight-medium">
                  {{ t('analytics.no_chart_data') }}
                </div>
              </div>
            </q-card-section>
            <q-inner-loading :showing="analyticsLoading">
              <q-spinner color="primary" />
            </q-inner-loading>
          </q-card>
        </div>
        <div class="col-12 col-md-4">
          <q-card class="shadow chart-card">
            <q-card-section>
              <div class="chart-title text-subtitle1 text-secondary text-weight-medium">
                {{ t('analytics.browsers_chart') }}
              </div>
              <div class="chart-container">
                <template v-if="hasBrowserData">
                  <apexchart type="donut" height="320" :series="browserSeries" :options="browserOptions" />
                </template>
                <div v-else class="chart-empty text-secondary text-weight-medium">
                  {{ t('analytics.no_chart_data') }}
                </div>
              </div>
            </q-card-section>
            <q-inner-loading :showing="analyticsLoading">
              <q-spinner color="primary" />
            </q-inner-loading>
          </q-card>
        </div>
        <div class="col-12 col-md-4">
          <q-card class="shadow chart-card">
            <q-card-section>
              <div class="chart-title text-subtitle1 text-secondary text-weight-medium">
                {{ t('analytics.device_types_chart') }}
              </div>
              <div class="chart-container">
                <template v-if="hasDeviceTypeData">
                  <apexchart type="donut" height="320" :series="deviceTypeSeries" :options="deviceTypeOptions" />
                </template>
                <div v-else class="chart-empty text-secondary text-weight-medium">
                  {{ t('analytics.no_chart_data') }}
                </div>
              </div>
            </q-card-section>
            <q-inner-loading :showing="analyticsLoading">
              <q-spinner color="primary" />
            </q-inner-loading>
          </q-card>
        </div>
      </div>

      <q-card class="shadow chart-card">
        <q-card-section>
          <div class="chart-title text-subtitle1 text-secondary text-weight-medium">
            {{ t('analytics.countries_chart') }}
          </div>
          <div class="chart-container">
            <template v-if="hasCountryData">
              <apexchart type="bar" height="320" :series="countriesChartSeries" :options="countriesChartOptions" />
            </template>
            <div v-else class="chart-empty text-secondary text-weight-medium">
              {{ t('analytics.no_chart_data') }}
            </div>
          </div>
        </q-card-section>
        <q-inner-loading :showing="analyticsLoading">
          <q-spinner color="primary" />
        </q-inner-loading>
      </q-card>

      <q-table
        v-model:pagination="pagination"
        :rows="scanRecords"
        :columns="columns"
        :loading="loading"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow analytics-table"
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
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import type { ApexOptions } from 'apexcharts';
import { computed, nextTick, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { mdiCalendarRange, mdiChartLine } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import QRCodeService from '@/api/services/QRCodeService';
import ScanRecordService, {
  type ScanAnalyticsResponse,
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
const analyticsLoading = ref(false);
const scanAnalytics = ref<ScanAnalyticsResponse | null>(null);

const numberFormatter = computed(() => new Intl.NumberFormat(locale.value || undefined));
const formattedPeriodScans = computed(() =>
  numberFormatter.value.format(scanAnalytics.value?.totalScansInPeriod ?? 0),
);
const formattedLifetimeScans = computed(() =>
  numberFormatter.value.format(scanAnalytics.value?.lifetimeScans ?? 0),
);
const scansLabel = computed(() => t('analytics.scans_label'));
type CategoryCount = { name: string; count: number };

const sortedDailyScans = computed(() => {
  const items = scanAnalytics.value?.dailyScans ?? [];
  return [...items].sort((left, right) => new Date(left.date).getTime() - new Date(right.date).getTime());
});

const dailyLineSeries = computed(() => {
  const label = scansLabel.value;
  const data = sortedDailyScans.value.map((item) => ({
    x: new Date(item.date).getTime(),
    y: item.count,
  }));
  return [
    {
      name: label,
      data,
    },
  ];
});

const hasDailyData = computed(() => sortedDailyScans.value.length > 0);

const dailyLineOptions = computed((): ApexOptions => {
  const currentLocale = locale.value || undefined;
  return {
    chart: {
      type: 'line',
      toolbar: { show: false },
      animations: { enabled: false },
    },
    stroke: {
      curve: 'smooth',
      width: 3,
    },
    markers: {
      size: 4,
      strokeWidth: 0,
    },
    dataLabels: {
      enabled: false,
    },
    xaxis: {
      type: 'datetime',
      labels: {
        datetimeUTC: false,
      },
    },
    yaxis: {
      min: 0,
      forceNiceScale: true,
      labels: {
        formatter(value: number) {
          const numeric = Number.isFinite(value) ? value : 0;
          return numberFormatter.value.format(Math.max(0, numeric));
        },
      },
    },
    tooltip: {
      shared: true,
      x: {
        formatter(value: number) {
          return new Date(value).toLocaleDateString(currentLocale, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
          });
        },
      },
      y: {
        formatter(value: number) {
          const numeric = Number.isFinite(value) ? value : 0;
          return numberFormatter.value.format(Math.max(0, numeric));
        },
      },
    },
    grid: {
      strokeDashArray: 4,
    },
  };
});

function sortCategoryCounts(items: CategoryCount[]): CategoryCount[] {
  return [...items].sort((left, right) => right.count - left.count);
}

const operatingSystemData = computed(() => sortCategoryCounts(scanAnalytics.value?.operatingSystems ?? []));
const operatingSystemLabels = computed(() => operatingSystemData.value.map((item) => item.name));
const operatingSystemSeries = computed(() => operatingSystemData.value.map((item) => item.count));
const hasOperatingSystemData = computed(() => operatingSystemSeries.value.length > 0);

const browserData = computed(() => sortCategoryCounts(scanAnalytics.value?.browsers ?? []));
const browserLabels = computed(() => browserData.value.map((item) => item.name));
const browserSeries = computed(() => browserData.value.map((item) => item.count));
const hasBrowserData = computed(() => browserSeries.value.length > 0);

const deviceTypeData = computed(() => sortCategoryCounts(scanAnalytics.value?.deviceTypes ?? []));
const deviceTypeLabels = computed(() => deviceTypeData.value.map((item) => item.name));
const deviceTypeSeries = computed(() => deviceTypeData.value.map((item) => item.count));
const hasDeviceTypeData = computed(() => deviceTypeSeries.value.length > 0);

const countriesData = computed(() => sortCategoryCounts(scanAnalytics.value?.countries ?? []));
const countryLabels = computed(() => countriesData.value.map((item) => item.name));
const countriesChartSeries = computed(() => [
  {
    name: scansLabel.value,
    data: countriesData.value.map((item) => item.count),
  },
]);
const hasCountryData = computed(() => countriesData.value.length > 0);

const operatingSystemOptions = computed((): ApexOptions => createDonutOptions(operatingSystemLabels.value));
const browserOptions = computed((): ApexOptions => createDonutOptions(browserLabels.value));
const deviceTypeOptions = computed((): ApexOptions => createDonutOptions(deviceTypeLabels.value));

const countriesChartOptions = computed((): ApexOptions => ({
  chart: {
    type: 'bar',
    toolbar: { show: false },
    animations: { enabled: false },
  },
  plotOptions: {
    bar: {
      columnWidth: '45%',
      borderRadius: 6,
    },
  },
  dataLabels: {
    enabled: false,
  },
  xaxis: {
    categories: countryLabels.value,
    labels: {
      rotateAlways: true,
      rotate: -35,
      trim: true,
    },
  },
  yaxis: {
    min: 0,
    forceNiceScale: true,
    labels: {
      formatter(value: number) {
        const numeric = Number.isFinite(value) ? value : 0;
        return numberFormatter.value.format(Math.max(0, numeric));
      },
    },
  },
  tooltip: {
    y: {
      formatter(value: number) {
        const numeric = Number.isFinite(value) ? value : 0;
        return numberFormatter.value.format(Math.max(0, numeric));
      },
    },
  },
  grid: {
    strokeDashArray: 4,
  },
  responsive: [
    {
      breakpoint: 1024,
      options: {
        plotOptions: {
          bar: {
            columnWidth: '55%',
          },
        },
      },
    },
    {
      breakpoint: 600,
      options: {
        chart: {
          height: 300,
        },
        xaxis: {
          labels: {
            rotate: -45,
          },
        },
      },
    },
  ],
}));

function createDonutOptions(labels: string[]): ApexOptions {
  const formatter = numberFormatter.value;
  return {
    chart: {
      type: 'donut',
    },
    labels,
    legend: {
      position: 'bottom',
    },
    dataLabels: {
      enabled: false,
    },
    tooltip: {
      y: {
        formatter(value: number) {
          const numeric = Number.isFinite(value) ? value : 0;
          return formatter.format(Math.max(0, numeric));
        },
      },
    },
    stroke: {
      width: 0,
    },
    responsive: [
      {
        breakpoint: 1024,
        options: {
          chart: {
            height: 300,
          },
        },
      },
      {
        breakpoint: 600,
        options: {
          chart: {
            height: 260,
          },
        },
      },
    ],
  };
}

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

async function fetchScanAnalytics() {
  if (!qrCodeId.value) {
    return;
  }

  const query = {
    QRCodeId: qrCodeId.value,
    StartDate: dateRangeIso.value.start,
    EndDate: dateRangeIso.value.end,
  };

  analyticsLoading.value = true;
  const { data, error } = await ScanRecordService.getScanAnalytics(query);
  analyticsLoading.value = false;

  if (error) {
    handleError(error, 'Failed to load analytics');
    return;
  }

  scanAnalytics.value = data ?? null;
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
    fetchScanAnalytics();
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
      scanAnalytics.value = null;
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

<style scoped lang="scss">
.analytics-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.metric-card {
  height: 100%;
}

.metric-card__section {
  min-height: 120px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.5rem;
}

.chart-card {
  position: relative;
}

.chart-container {
  width: 100%;
  min-height: 240px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-empty {
  width: 100%;
  padding: 1.5rem 0;
}

.analytics-table {
  margin-top: 0.5rem;
}

@media (max-width: 768px) {
  .chart-container {
    min-height: 200px;
  }
}
</style>
