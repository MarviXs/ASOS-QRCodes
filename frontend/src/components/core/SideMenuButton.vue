<template>
  <router-link :to="to" :exact="false" :class="{ active: subIsActive() }">
    <q-btn no-caps flat :label="label" align="left" class="menu-btn q-my-xs" :icon="icon" size="15px" padding="10px">
    </q-btn
  ></router-link>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';

const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  icon: {
    type: String,
    required: true,
  },
  to: {
    type: String,
    required: true,
  },
  exact: {
    type: Boolean,
    default: false,
  },
});

const route = useRoute();

const subIsActive = () => {
  if (props.to === '/' && route.path !== '/') {
    return false;
  }

  const paths = Array.isArray(props.to) ? props.to : [props.to];
  return paths.some((path) => route.path.indexOf(path) === 0);
};
</script>

<style lang="scss" scoped>
.menu-btn {
  width: 100%;
  border-radius: 12px;
  font-weight: 500;
  color: #8296bd;
  i {
    color: #9eb3d3;
  }
}

.active {
  .q-btn {
    background-color: #edf1fc;
    color: $primary;
  }
}
</style>
