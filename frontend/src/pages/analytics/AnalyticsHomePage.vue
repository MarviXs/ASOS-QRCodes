<template>
  <ScanAnalyticsView :breadcrumbs="breadcrumbs" include-qr-code-column />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ScanAnalyticsView from '@/components/analytics/ScanAnalyticsView.vue';

const { t } = useI18n();

const breadcrumbs = computed(() => [
  { label: t('global.analytics') },
]);
</script>
