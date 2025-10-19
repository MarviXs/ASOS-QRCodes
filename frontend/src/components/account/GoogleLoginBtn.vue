<template>
  <GoogleLogin :callback="login" prompt />
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth-store';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { toast } from 'vue3-toastify';

const { t } = useI18n();

const loading = defineModel('loading', {
  type: Boolean,
  required: true,
});

const authStore = useAuthStore();
const router = useRouter();

async function login(response: { credential: string }) {
  loading.value = true;
  const { error } = await authStore.loginByGoogle(response.credential);
  loading.value = false;

  if (error) {
    handleError(error, t('auth.login.toasts.login_failed'));
    return;
  }

  toast.success(t('auth.login.toasts.login_success'));
  router.push('/');
}
</script>
