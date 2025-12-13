<template>
  <PageLayout :breadcrumbs="[{ label: t('global.qr_codes'), to: '/qr-codes' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('qrcode.actions.create')"
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

        <template #body-cell-redirectUrl="props">
          <q-td :props="props">
            <a
              v-if="props.row.redirectUrl"
              class="qr-url"
              :href="normalizeHref(props.row.redirectUrl, props.row.shortCode)"
              target="_blank"
              rel="noopener noreferrer"
            >
              {{ formatUrl(props.row.redirectUrl, props.row.shortCode) }}
            </a>
            <span v-else class="qr-url--missing">—</span>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :icon="mdiChartLine" color="grey-color" flat round :to="`/qr-codes/${props.row.id}/analytics`">
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.analytics') }}
              </q-tooltip>
            </q-btn>
            <q-btn :icon="mdiDownload" color="grey-color" flat round @click="downloadQRCode(props.row)">
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.download') }}
              </q-tooltip>
            </q-btn>
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
import { mdiChartLine, mdiCodeTags, mdiPencil, mdiPlus, mdiDownload, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
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
import { buildScanUrl } from '@/utils/qr-url';
import QRCodeStyling, { type CornerDotType, type CornerSquareType, type DotType } from 'qr-code-styling';

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

function normalizeHref(url: string, shortCode: string) {
  const trimmed = url?.trim?.() ?? '';
  if (!trimmed) {
    return buildScanUrl(shortCode);
  }
  if (/^https?:\/\//i.test(trimmed)) {
    return trimmed;
  }
  return `https://${trimmed}`;
}

function formatUrl(url: string, shortCode: string) {
  const href = normalizeHref(url, shortCode);
  try {
    const parsed = new URL(href);
    return parsed.host + parsed.pathname.replace(/\/$/, '');
  } catch {
    return href.replace(/^https?:\/\//i, '');
  }
}

async function downloadQRCode(row: {
  dotStyle: string;
  cornerDotStyle: string;
  cornerSquareStyle: string;
  color: string;
  shortCode: string;
  displayName: any;
}) {
  const dotStyle = (row.dotStyle as DotType) || 'square';
  const cornerDotStyle = (row.cornerDotStyle as CornerDotType) || 'square';
  const cornerSquareStyle = (row.cornerSquareStyle as CornerSquareType) || 'square';
  const color = row.color || '#000000';

  const qrInstance = new QRCodeStyling({
    width: 500,
    height: 500,
    data: buildScanUrl(row.shortCode),
    dotsOptions: {
      type: dotStyle,
      color,
    },
    cornersDotOptions: {
      type: cornerDotStyle,
      color,
    },
    cornersSquareOptions: {
      type: cornerSquareStyle,
      color,
    },
    backgroundOptions: {
      color: '#ffffff',
    },
    imageOptions: {
      crossOrigin: 'anonymous',
    },
  });

  const rawData = await qrInstance.getRawData('png');

  const blob: Blob =
    rawData instanceof Blob ? rawData : new Blob([rawData as unknown as BlobPart], { type: 'image/png' });

  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = `${row.displayName || row.shortCode || 'qr-code'}.png`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}

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
    label: t('qrcode.table.qr_code'),
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
    label: t('qrcode.table.updated_at'),
    field: 'updatedAt',
    sortable: true,
    format(val) {
      return new Date(val).toLocaleString(locale.value);
    },
    align: 'right',
  },

  {
    name: 'createdAt',
    label: t('qrcode.table.created_at'),
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

<style scoped>
.qr-url {
  color: var(--q-primary);
  text-decoration: none;
}

.qr-url:hover {
  text-decoration: underline;
}

.qr-url--missing {
  color: rgba(0, 0, 0, 0.4);
}
</style>
