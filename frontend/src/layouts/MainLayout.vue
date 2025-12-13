<template>
    <q-layout view="lHh LpR lfr">
      <q-header class="bg-white text-secondary shadow">
        <q-toolbar>
        <q-btn flat dense round :icon="mdiMenu" :aria-label="t('global.menu')" @click="toggleLeftDrawer" />
        <q-space />
        <language-select class="q-mr-md"></language-select>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above class="shadow bg-white">
      <div class="column q-px-lg q-pt-lg q-pb-xl full-height no-wrap drawer-content">
        <router-link class="q-my-lg q-mx-auto full-width" to="/">
          <q-img src="../assets/logo.png" height="3.7rem" fit="contain" no-spinner no-transition />
        </router-link>
        <div class="links">
          <side-menu-button to="/" :label="t('global.analytics')" :icon="mdiChartLine" />
          <side-menu-button to="/qr-codes" :label="t('global.qr_codes')" :icon="mdiQrcode" />
        </div>
        <q-separator class="q-my-sm" />
        <div class="links user-links">
          <side-menu-button to="/account" :label="t('global.account')" :icon="mdiAccountOutline" />
          <side-menu-action-button :label="t('account.logout')" :icon="mdiLogout" @click="logout" />
        </div>
      </div>
    </q-drawer>
    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import SideMenuActionButton from '@/components/core/SideMenuActionButton.vue';
import SideMenuButton from '@/components/core/SideMenuButton.vue';
import LanguageSelect from '@/components/core/LanguageSelect.vue';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import { mdiMenu, mdiLogout, mdiAccountOutline, mdiQrcode, mdiChartLine } from '@quasar/extras/mdi-v7';
import { toast } from 'vue3-toastify';

const { t } = useI18n();

const authStore = useAuthStore();
const leftDrawerOpen = ref(false);

function logout() {
  authStore.logout();
  toast.success(t('auth.toasts.logout_success'));
}

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value;
}
</script>

<style scoped lang="scss">
.drawer-content {
  height: 100%;
}

.links {
  display: flex;
  flex-direction: column;
}

.user-links {
  margin-bottom: 1.5rem;
}
</style>
