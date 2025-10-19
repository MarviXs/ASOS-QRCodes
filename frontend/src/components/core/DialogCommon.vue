<template>
  <q-dialog v-model="isDialogOpen" v-bind="attrs">
    <q-card class="q-pa-sm" :style="dialogStyle">
      <q-card-section>
        <div class="text-h6">
          <slot name="title" />
        </div>
      </q-card-section>
      <slot />
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed, useAttrs } from 'vue';
import { useQuasar } from 'quasar';
const props = defineProps({
  minWidth: {
    type: String,
    default: '380px',
  },
});
const isDialogOpen = defineModel<boolean>({ default: false });

const attrs = useAttrs();

const quasar = useQuasar();

const dialogStyle = computed(() => ({
  minWidth: quasar.screen.gt.sm ? props.minWidth : undefined,
  width: !quasar.screen.gt.sm ? '100%' : undefined,
}));
</script>
