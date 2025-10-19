<template>
  <PageLayout :breadcrumbs="[{ label: 'QR Codes', to: '/qr-codes' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        label="Create QR Code"
        :icon="mdiPlus"
        to="/qr-codes/create"
      />
    </template>
    <template #default>
      <q-table
        v-model:pagination="pagination"
        :rows="qrCodes"
        :columns="columns"
        :loading="loadingQR"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        @request="(requestProp) => getQRcodes(requestProp.pagination)"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #body-cell-qrCode="props">
          <q-td :props="props" auto-width>
            <QRCodePreview
              :short-code="props.row.shortCode"
              :display-name="props.row.displayName"
              :dot-style="props.row.dotStyle"
              :corner-dot-style="props.row.cornerDotStyle"
              :corner-square-style="props.row.cornerSquareStyle"
              :color="props.row.color"
            />
          </q-td>
        </template>

        <template #body-cell-name="props">
          <q-td :props="props">
            <RouterLink :to="`/qr-codes/${props.row.id}/edit`">{{ props.row.displayName }}</RouterLink>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :icon="mdiPencil" color="grey-color" flat round :to="`/qr-codes/${props.row.id}/edit`">
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
            <q-btn :icon="mdiTrashCanOutline" color="grey-color" flat round @click="openDeleteDialog(props.row.id)"
              ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.delete') }}
              </q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
  <DeleteQRCodeDialog v-model="deleteDialogOpen" :qrcode-id="deleteQRCodeId" @on-deleted="getQRcodes(pagination)" />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiCodeTags, mdiPencil, mdiPlus, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import QRCodeService from '@/api/services/QRCodeService';
import { handleError } from '@/utils/error-handler';
import type { QRCodeQueryParams, QRCodeResponse } from '@/api/services/QRCodeService';
import type { QTableProps } from 'quasar';
import { watchDebounced } from '@vueuse/core';
import DeleteQRCodeDialog from '@/components/qr-codes/DeleteQRCodeDialog.vue';
import QRCodePreview from '@/components/qr-codes/QRCodePreview.vue';

const { t, locale } = useI18n();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});
const qrCodesPaginated = ref<QRCodeResponse>();
const qrCodes = computed(() => qrCodesPaginated.value?.items ?? []);

const loadingQR = ref(false);
const deleteDialogOpen = ref(false);
const deleteQRCodeId = ref<string>();

async function getQRcodes(paginationTable: PaginationTable) {
  const paginationQuery: QRCodeQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingQR.value = true;
  const { data, error } = await QRCodeService.getQRCodes(paginationQuery);
  loadingQR.value = false;

  if (error) {
    handleError(error, 'Loading QR codes failed');
    return;
  }

  qrCodesPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getQRcodes(pagination.value);

function openDeleteDialog(id: string) {
  deleteQRCodeId.value = id;
  deleteDialogOpen.value = true;
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'qrCode',
    label: 'QR Code',
    field: 'shortCode',
    sortable: false,
    align: 'left',
  },
  {
    name: 'displayName',
    label: t('global.name'),
    field: 'displayName',
    sortable: true,
    align: 'left',
  },

  {
    name: 'redirectUrl',
    label: t('global.url'),
    field: 'redirectUrl',
    sortable: false,
    align: 'left',
  },

  {
    name: 'updatedAt',
    label: 'Updated At',
    field: 'updatedAt',
    sortable: true,
    format(val) {
      return new Date(val).toLocaleString(locale.value);
    },
    align: 'right',
  },

  {
    name: 'createdAt',
    label: 'Created At',
    field: 'createdAt',
    sortable: true,
    format(val) {
      return new Date(val).toLocaleString(locale.value);
    },
    align: 'right',
  },

  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

watchDebounced(filter, () => getQRcodes(pagination.value), { debounce: 400 });
</script>
