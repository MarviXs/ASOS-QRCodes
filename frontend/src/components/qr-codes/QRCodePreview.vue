<template>
  <div class="qr-code-preview">
    <div ref="qrContainer" class="qr-code-preview__canvas"></div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import QRCodeStyling, { type CornerDotType, type CornerSquareType, type DotType } from 'qr-code-styling';
import { buildScanUrl } from '@/utils/qr-url';

const PREVIEW_SIZE = 70;

const props = defineProps<{
  shortCode: string;
  displayName: string;
  dotStyle: DotType;
  cornerDotStyle: CornerDotType;
  cornerSquareStyle: CornerSquareType;
  color: string;
}>();

const qrContainer = ref<HTMLDivElement | null>(null);
const qrInstance = ref<QRCodeStyling>();

const normalizedColor = computed(() => props.color || '#000000');
const encodedUrl = computed(() => buildScanUrl(props.shortCode));

function renderQrCode() {
  if (!qrInstance.value) {
    return;
  }

  qrInstance.value.update({
    data: encodedUrl.value,
    dotsOptions: {
      type: props.dotStyle || 'square',
      color: normalizedColor.value,
    },
    cornersDotOptions: {
      type: props.cornerDotStyle || 'square',
      color: normalizedColor.value,
    },
    cornersSquareOptions: {
      type: props.cornerSquareStyle || 'square',
      color: normalizedColor.value,
    },
  });
}

onMounted(() => {
  const instance = new QRCodeStyling({
    width: PREVIEW_SIZE,
    height: PREVIEW_SIZE,
    data: encodedUrl.value,
    dotsOptions: {
      type: props.dotStyle || 'square',
      color: normalizedColor.value,
    },
    cornersDotOptions: {
      type: props.cornerDotStyle || 'square',
      color: normalizedColor.value,
    },
    cornersSquareOptions: {
      type: props.cornerSquareStyle || 'square',
      color: normalizedColor.value,
    },
    imageOptions: {
      crossOrigin: 'anonymous',
    },
    backgroundOptions: {
      color: '#ffffff',
    },
  });

  qrInstance.value = instance;

  if (qrContainer.value) {
    qrContainer.value.innerHTML = '';
    instance.append(qrContainer.value);
  }

  renderQrCode();
});

watch(
  () => [encodedUrl.value, props.dotStyle, props.cornerDotStyle, props.cornerSquareStyle, props.color],
  () => {
    renderQrCode();
  },
  { flush: 'post' },
);

onBeforeUnmount(() => {
  if (qrContainer.value) {
    qrContainer.value.innerHTML = '';
  }
  qrInstance.value = undefined;
});
</script>

<style scoped>
.qr-code-preview {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
