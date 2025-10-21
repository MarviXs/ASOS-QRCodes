<template>
  <q-btn
    no-caps
    flat
    :label="label"
    align="left"
    class="menu-btn q-my-xs"
    :icon="icon"
    size="15px"
    padding="10px"
    @click="emit('click')"
  />
</template>

<script setup lang="ts">
defineProps<{
  label: string;
  icon: string;
}>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();
</script>

<style lang="scss" scoped>
.menu-btn {
  width: 100%;
  border-radius: 12px;
  font-weight: 500;
  color: #8296bd;
  justify-content: flex-start;

  .q-btn__content {
    justify-content: flex-start;
  }

  i {
    color: #9eb3d3;
  }
}
</style>
