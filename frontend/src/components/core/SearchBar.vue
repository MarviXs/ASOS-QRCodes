<template>
  <q-input v-model="filter" :placeholder="t('global.search')" class="shadow" filled bg-color="white" dense>
    <template #append>
      <q-icon v-if="filter === ''" :name="mdiMagnify" />
      <q-icon v-else :name="mdiClose" class="cursor-pointer" @click="filter = ''" />
    </template>
  </q-input>
</template>

<script setup lang="ts">
import { mdiMagnify, mdiClose } from '@quasar/extras/mdi-v7';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const filter = defineModel({
  type: String,
  default: '',
});
</script>
