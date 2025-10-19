<template>
  <PageLayout
    :breadcrumbs="[
      { label: 'QR Codes', to: '/qr-codes' },
      { label: 'Create QR Code', to: '/qr-codes/create' },
    ]"
  >
    <template #default>
      <QRCodeForm v-if="!isFetchingQR" v-model="qrCodeData" :isLoading="isLoading" @onSubmit="updateQRCode" />
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import QRCodeService from '@/api/services/QRCodeService';
import QRCodeForm, { type QRCodeFormData } from '@/components/qr-codes/QRCodeForm.vue';
import PageLayout from '@/layouts/PageLayout.vue';
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { toast } from 'vue3-toastify';

const router = useRouter();
const qrCodeId = router.currentRoute.value.params.id as string;

const qrCodeData = ref<QRCodeFormData>({
  displayName: '',
  redirectUrl: '',
  shortCode: '',
  dotStyle: 'square',
  cornerDotStyle: 'square',
  cornerSquareStyle: 'square',
  color: '#000000',
});
const isLoading = ref(false);

const isFetchingQR = ref(false);
async function fetchQRCode() {
  isFetchingQR.value = true;
  const { data, error } = await QRCodeService.getQRCode(qrCodeId);
  isFetchingQR.value = false;

  if (error) {
    handleError(error, 'Failed to load QR Code data');
    return;
  }

  qrCodeData.value.displayName = data.displayName;
  qrCodeData.value.redirectUrl = data.redirectUrl;
  qrCodeData.value.shortCode = data.shortCode;
  qrCodeData.value.dotStyle = (data.dotStyle ?? 'square') as typeof qrCodeData.value.dotStyle;
  qrCodeData.value.cornerDotStyle = (data.cornerDotStyle ?? 'square') as typeof qrCodeData.value.cornerDotStyle;
  qrCodeData.value.cornerSquareStyle = (data.cornerSquareStyle ?? 'square') as typeof qrCodeData.value.cornerSquareStyle;
  qrCodeData.value.color = data.color ?? '#000000';
}
fetchQRCode();

async function updateQRCode(data: QRCodeFormData) {
  isLoading.value = true;
  const submitResponse = await QRCodeService.updateQRCode(qrCodeId, {
    displayName: data.displayName,
    redirectUrl: data.redirectUrl,
    shortCode: data.shortCode,
    dotStyle: data.dotStyle,
    cornerDotStyle: data.cornerDotStyle,
    cornerSquareStyle: data.cornerSquareStyle,
    color: data.color,
  });

  isLoading.value = false;

  if (submitResponse.error) {
    handleError(submitResponse.error, 'Failed to update QR Code');
    return;
  }
  toast.success('QR Code updated successfully!');
  router.push(`/qr-codes`);
}
</script>
