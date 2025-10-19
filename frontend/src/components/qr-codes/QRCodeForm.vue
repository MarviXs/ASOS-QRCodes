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
          <div class="shape-selector q-mb-xl">
            <div class="shape-group q-mb-lg">
              <div class="shape-group__title text-subtitle2 text-weight-medium q-mb-sm">Body</div>
              <div class="shape-group__options">
                <div
                  v-for="option in bodyShapeOptions"
                  :key="option.value"
                  class="shape-option"
                  :class="{ 'shape-option--selected': qrCodeData.dotStyle === option.value }"
                  role="button"
                  tabindex="0"
                  :aria-pressed="qrCodeData.dotStyle === option.value"
                  @click="selectBodyShape(option.value)"
                  @keyup.enter.prevent="selectBodyShape(option.value)"
                  @keydown.space.prevent="selectBodyShape(option.value)"
                >
                  <q-img :src="option.image" :alt="option.label" fit="contain" />
                </div>
              </div>
            </div>
            <div class="shape-group q-mb-lg">
              <div class="shape-group__title text-subtitle2 text-weight-medium q-mb-sm">Outer Eye</div>
              <div class="shape-group__options">
                <div
                  v-for="option in outerEyeOptions"
                  :key="option.value"
                  class="shape-option"
                  :class="{ 'shape-option--selected': qrCodeData.cornerSquareStyle === option.value }"
                  role="button"
                  tabindex="0"
                  :aria-pressed="qrCodeData.cornerSquareStyle === option.value"
                  @click="selectOuterEye(option.value)"
                  @keyup.enter.prevent="selectOuterEye(option.value)"
                  @keydown.space.prevent="selectOuterEye(option.value)"
                >
                  <q-img :src="option.image" :alt="option.label" fit="contain" />
                </div>
              </div>
            </div>
            <div class="shape-group">
              <div class="shape-group__title text-subtitle2 text-weight-medium q-mb-sm">Inner Eye</div>
              <div class="shape-group__options">
                <div
                  v-for="option in innerEyeOptions"
                  :key="option.value"
                  class="shape-option"
                  :class="{ 'shape-option--selected': qrCodeData.cornerDotStyle === option.value }"
                  role="button"
                  tabindex="0"
                  :aria-pressed="qrCodeData.cornerDotStyle === option.value"
                  @click="selectInnerEye(option.value)"
                  @keyup.enter.prevent="selectInnerEye(option.value)"
                  @keydown.space.prevent="selectInnerEye(option.value)"
                >
                  <q-img :src="option.image" :alt="option.label" fit="contain" />
                </div>
              </div>
            </div>
          </div>
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
import { onMounted, ref, watch } from 'vue';
import QRCodeStyling, { type DotType } from 'qr-code-styling';

export interface QRCodeFormData {
  displayName: string;
  redirectUrl: string;
  shortCode: string;
  dotStyle: DotType;
  cornerDotStyle: DotType;
  cornerSquareStyle: DotType;
  color: string;
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
      color: qrCodeData.value.color || '#000000',
      type: qrCodeData.value.dotStyle || 'square',
    },
    cornersSquareOptions: {
      type: qrCodeData.value.cornerSquareStyle || 'square',
    },
    cornersDotOptions: {
      type: qrCodeData.value.cornerDotStyle || 'square',
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

const bodyShapeOptions = [
  {
    label: 'Square',
    value: 'square',
    image: new URL('../../assets/shapes/body/square.png', import.meta.url).href,
  },
  {
    label: 'Rounded',
    value: 'rounded',
    image: new URL('../../assets/shapes/body/rounded.png', import.meta.url).href,
  },
  {
    label: 'Dots',
    value: 'dots',
    image: new URL('../../assets/shapes/body/dots.png', import.meta.url).href,
  },
  {
    label: 'Classy',
    value: 'classy',
    image: new URL('../../assets/shapes/body/classy.png', import.meta.url).href,
  },
  {
    label: 'Classy Rounded',
    value: 'classy-rounded',
    image: new URL('../../assets/shapes/body/classy_rounded.png', import.meta.url).href,
  },
  {
    label: 'Extra Rounded',
    value: 'extra-rounded',
    image: new URL('../../assets/shapes/body/extra_rounded.png', import.meta.url).href,
  },
] as const;

const outerEyeOptions = [
  {
    label: 'Square',
    value: 'square',
    image: new URL('../../assets/shapes/outer_eye/square.png', import.meta.url).href,
  },
  {
    label: 'Dot',
    value: 'dot',
    image: new URL('../../assets/shapes/outer_eye/dot.png', import.meta.url).href,
  },
  {
    label: 'Extra Rounded',
    value: 'extra-rounded',
    image: new URL('../../assets/shapes/outer_eye/extra_rounded.png', import.meta.url).href,
  },
] as const;

const innerEyeOptions = [
  {
    label: 'Square',
    value: 'square',
    image: new URL('../../assets/shapes/inner_eye/square.png', import.meta.url).href,
  },
  {
    label: 'Dot',
    value: 'dot',
    image: new URL('../../assets/shapes/inner_eye/dot.png', import.meta.url).href,
  },
] as const;

function selectBodyShape(value: (typeof bodyShapeOptions)[number]['value']) {
  qrCodeData.value.dotStyle = value;
}

function selectOuterEye(value: (typeof outerEyeOptions)[number]['value']) {
  qrCodeData.value.cornerSquareStyle = value as DotType;
}

function selectInnerEye(value: (typeof innerEyeOptions)[number]['value']) {
  qrCodeData.value.cornerDotStyle = value as DotType;
}

function handleSubmit() {
  emit('onSubmit', qrCodeData.value);
}

function updateQRCodePreview() {
  qrCodeStyle.value.update({
    data: `https://qr.smogrovic.com/code/${qrCodeData.value.shortCode}`,
    dotsOptions: {
      color: qrCodeData.value.color || '#000000',
      type: qrCodeData.value.dotStyle || 'square',
    },
    cornersSquareOptions: {
      type: qrCodeData.value.cornerSquareStyle || 'square',
    },
    cornersDotOptions: {
      type: qrCodeData.value.cornerDotStyle || 'square',
    },
  });
}

watch(
  () => [
    qrCodeData.value.dotStyle,
    qrCodeData.value.cornerSquareStyle,
    qrCodeData.value.cornerDotStyle,
    qrCodeData.value.shortCode,
    qrCodeData.value.color,
  ],
  updateQRCodePreview,
);

onMounted(() => {
  if (qrcodeRef.value) {
    console.log('Appending QR code to the DOM element.');
    qrCodeStyle.value.append(qrcodeRef.value);
    updateQRCodePreview();
  }
});
</script>

<style scoped>
.shape-selector {
  border-top: 1px solid rgba(0, 0, 0, 0.08);
  padding-top: 16px;
}

.shape-group__options {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.shape-option {
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 8px;
  padding: 6px;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease;
  outline: none;
  cursor: pointer;
}

.shape-option:focus-visible {
  border-color: var(--q-primary);
  box-shadow: 0 0 0 2px rgba(41, 182, 246, 0.3);
}

.shape-option--selected {
  border-color: var(--q-primary);
  box-shadow: 0 0 0 2px rgba(41, 182, 246, 0.3);
}

.shape-option :deep(.q-img__content > div) {
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
