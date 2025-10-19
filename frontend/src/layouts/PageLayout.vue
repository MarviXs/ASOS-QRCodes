<template>
  <q-page class="main-padding">
    <div>
      <div class="q-mb-md row items-center">
        <div class="bread-crumb row items-center">
          <div v-if="breadcrumbs && breadcrumbs.length > 1" class="row items-center">
            <div v-for="(item, index) in breadcrumbs.slice(0, -1)" :key="index" class="row items-center">
              <template v-if="item.label && item.to">
                <router-link class="title-text text-accent text-weight-medium z-fab" :to="item.to">
                  {{ item.label }}
                </router-link>
              </template>
              <template v-else>
                <q-skeleton width="4rem" />
              </template>
              <q-icon class="arrow-icon" :name="mdiChevronRight" />
            </div>
          </div>
          <p class="title-text current-item">{{ currentItem.label }}</p>
          <slot name="after-title" />
        </div>
        <slot name="description" />
        <q-space></q-space>
        <div class="actions">
          <slot name="actions" />
        </div>
      </div>
      <slot />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { mdiChevronRight } from '@quasar/extras/mdi-v7';
import type { PropType } from 'vue';
import { computed } from 'vue';

interface BreadCrumb {
  label?: string;
  to?: string;
}

const props = defineProps({
  breadcrumbs: {
    type: Array as PropType<BreadCrumb[]>,
    required: true,
  },
});

const currentItem = computed(() => {
  return props.breadcrumbs[props.breadcrumbs.length - 1];
});
</script>

<style lang="scss" scoped>
.z-index {
  z-index: 1001;
}

.actions {
  display: flex;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
}

.title-text {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
  color: $secondary;
}

.current-item {
  margin-right: 0.4rem;
}

.bread-crumb {
  margin-bottom: 0.3rem;
}

.arrow-icon {
  font-size: 2em;
  color: $accent;
}

@media (max-width: 800px) {
  .title-text {
    font-size: 1.3rem;
  }
}

@media (max-width: 600px) {
  .title-text {
    font-size: 1rem;
  }
}
</style>
