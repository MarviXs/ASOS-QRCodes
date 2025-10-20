<template>
  <ScanAnalyticsView :qr-code-id="qrCodeId" :breadcrumbs="breadcrumbs" />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import ScanAnalyticsView from '@/components/analytics/ScanAnalyticsView.vue';
import QRCodeService from '@/api/services/QRCodeService';
import { handleError } from '@/utils/error-handler';

const route = useRoute();
const { t } = useI18n();

const qrCodeId = ref<string | undefined>(typeof route.params.id === 'string' ? route.params.id : undefined);
const qrCodeName = ref<string>('');

const breadcrumbs = computed(() => [
  { label: 'QR Codes', to: '/qr-codes' },
  { label: qrCodeName.value || t('analytics.title') },
]);

async function fetchQRCodeName(currentId: string) {
  const { data, error } = await QRCodeService.getQRCode(currentId);
  if (error) {
    handleError(error, 'Failed to load QR code');
    return;
  }
  qrCodeName.value = data?.displayName ?? '';
}

watch(
  () => route.params.id,
  (newId) => {
    if (typeof newId === 'string') {
      qrCodeId.value = newId;
      fetchQRCodeName(newId);
    } else {
      qrCodeId.value = undefined;
      qrCodeName.value = '';
    }
  },
  { immediate: true },
);
</script>
