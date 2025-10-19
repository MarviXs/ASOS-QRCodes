<template>
  <q-dialog v-model="isDialogOpen">
    <q-card>
      <q-card-section>
        <div class="text-h6">
          <slot name="title" />
        </div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        <slot name="description" />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat color="grey-9" :label="t('global.cancel')" no-caps />
        <q-btn
          unelevated
          :label="t('global.delete')"
          color="red"
          :loading="loading"
          no-caps
          @click="emit('onSubmit')"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const isDialogOpen = defineModel<boolean>();
defineProps({
  loading: {
    type: Boolean,
    required: true,
  },
});
const emit = defineEmits(['onSubmit']);

const { t } = useI18n();
</script>

<style lang="scss" scoped></style>
