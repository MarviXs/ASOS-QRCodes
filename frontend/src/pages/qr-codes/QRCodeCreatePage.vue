<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('global.qr_codes'), to: '/qr-codes' },
      { label: t('qrcode.actions.create'), to: '/qr-codes/create' },
    ]"
  >
    <template #default>
      <QRCodeForm
        v-model="qrCodeData"
        :isLoading="isLoading"
        :submit-label="t('qrcode.actions.create')"
        @onSubmit="createQRCode"
      />
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
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';

const router = useRouter();
const { t } = useI18n();

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
  dotStyle: 'square',
  cornerDotStyle: 'square',
  cornerSquareStyle: 'square',
  color: '#000000',
});
const isLoading = ref(false);

async function createQRCode(data: QRCodeFormData) {
  isLoading.value = true;
  const submitResponse = await QRCodeService.createQRCode({
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
    handleError(submitResponse.error, 'Failed to create QR Code');
    return;
  }

  toast.success(t('qrcode.toasts.create_success'));
  router.push(`/qr-codes`);
}
</script>
