<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('qrcode.delete_qrcode') }}</template>
    <template #description>{{ t('qrcode.delete_qrcode_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import QRCodeService from '@/api/services/QRCodeService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  qrcodeId: {
    type: String,
    default: '',
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);

async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await QRCodeService.deleteQRCode(props.qrcodeId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, 'Error deleting QR code');
    return;
  }

  toast.success(t('qrcode.toasts.delete_success'));
  emit('onDeleted');
  isDialogOpen.value = false;
}
</script>
