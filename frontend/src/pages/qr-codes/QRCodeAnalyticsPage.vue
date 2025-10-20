<template>
  <PageLayout :breadcrumbs="breadcrumbs">
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
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { mdiChartLine } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import QRCodeService from '@/api/services/QRCodeService';
import ScanRecordService, {
  type ScanRecordsQueryParams,
  type ScanRecordsResponse,
} from '@/api/services/ScanRecordService';
import type { QTableProps } from 'quasar';

const route = useRoute();
const { t } = useI18n();

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

const breadcrumbs = computed(() => [{ label: 'QR Codes', to: '/qr-codes' }, { label: qrCodeName.value || '' }]);

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

watch(
  () => route.params.id,
  (newId) => {
    if (typeof newId === 'string') {
      qrCodeId.value = newId;
      pagination.value.page = 1;
      fetchQRCodeName(newId);
      fetchScanRecords(pagination.value);
    }
  },
  { immediate: true },
);
</script>
