<template>
  <div class="row q-col-gutter-x-xl q-col-gutter-y-lg">
    <div class="col-12 col-md-6 col-lg-8">
      <q-card class="shadow q-px-lg q-py-lg">
        <q-form @submit.prevent="handleSubmit">
          <q-input v-model="qrCodeData.displayName" label="Display Name" class="q-mb-md">
            <template v-slot:prepend>
              <q-icon :name="mdiPencil" />
            </template>
          </q-input>
          <q-input v-model="qrCodeData.redirectUrl" label="URL" class="q-mb-md">
            <template v-slot:prepend>
              <q-icon :name="mdiLinkVariant" />
            </template>
          </q-input>
          <div class="row justify-end">
            <q-btn
              label="Create QR Code"
              color="primary"
              unelevated
              size="15px"
              no-caps
              :loading="isLoading"
              type="submit"
            />
          </div>
        </q-form>
      </q-card>
    </div>
    <div class="col-12 col-md-6 col-lg-4">
      <q-card class="shadow q-px-lg q-py-xl">
        <div class="column flex-center">
          <div class="qrcode" id="qrcode" ref="qrcodeRef"></div>
        </div>
      </q-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { mdiLinkVariant, mdiPencil } from '@quasar/extras/mdi-v7';
import { onMounted, ref } from 'vue';
import QRCodeStyling from 'qr-code-styling';

export interface QRCodeFormData {
  displayName: string;
  redirectUrl: string;
  shortCode: string;
}
defineProps({
  isLoading: {
    type: Boolean,
    required: true,
  },
});
const emit = defineEmits(['onSubmit']);
const qrCodeData = defineModel<QRCodeFormData>();

const qrcodeRef = ref<HTMLElement | null>(null);
const qrCodeStyle = ref<QRCodeStyling>(
  new QRCodeStyling({
    width: 200,
    height: 200,
    data: `https://qr.smogrovic.com/code/${qrCodeData.value.shortCode}`,
    dotsOptions: {
      color: '#000000',
    },
    backgroundOptions: {
      color: '#ffffff',
    },
    imageOptions: {
      crossOrigin: 'anonymous',
      margin: 20,
    },
  }),
);

function handleSubmit() {
  emit('onSubmit', qrCodeData.value);
}

onMounted(() => {
  if (qrcodeRef.value) {
    console.log('Appending QR code to the DOM element.');
    qrCodeStyle.value.append(qrcodeRef.value);
  }
});
</script>
