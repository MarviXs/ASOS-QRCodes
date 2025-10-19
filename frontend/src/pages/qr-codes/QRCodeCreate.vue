<template>
  <PageLayout
    :breadcrumbs="[
      { label: 'QR Codes', to: '/qr-codes' },
      { label: 'Create QR Code', to: '/qr-codes/create' },
    ]"
  >
    <template #default>
      <QRCodeForm v-model="qrCodeData" :isLoading="isLoading" @onSubmit="createQRCode" />
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

function generateShortCode() {
  const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  for (let i = 0; i < 6; i++) {
    result += characters.charAt(Math.floor(Math.random() * characters.length));
  }
  return result;
}

const qrCodeData = ref<QRCodeFormData>({
  displayName: '',
  redirectUrl: '',
  shortCode: generateShortCode(),
});
const isLoading = ref(false);

async function createQRCode(data: QRCodeFormData) {
  isLoading.value = true;
  const submitResponse = await QRCodeService.createQRCode({
    displayName: data.displayName,
    redirectUrl: data.redirectUrl,
    shortCode: data.shortCode,
  });
  isLoading.value = false;

  if (submitResponse.error) {
    handleError(submitResponse.error, 'Failed to create QR Code');
    return;
  }

  toast.success('QR Code created successfully!');
  router.push(`/qr-codes`);
}
</script>
