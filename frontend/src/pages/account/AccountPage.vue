<template>
  <PageLayout :breadcrumbs="[{ label: t('global.account'), to: '/account' }]">
    <template #default>
      <div class="row justify-center">
        <div class="col-12 col-md-8 col-lg-6">
          <q-card class="shadow q-px-lg q-py-xl">
            <q-card-section class="q-py-lg">
              <div class="text-h6">{{ t('account.password_title') }}</div>
              <p class="text-body2 q-mt-sm q-mb-lg text-weight-regular">
                {{ t('account.password_description') }}
              </p>
            </q-card-section>
            <q-separator />
            <q-card-section class="q-pt-lg">
              <q-form class="q-gutter-md" @submit.prevent="changePassword">
                <q-input
                  v-model="form.currentPassword"
                  :label="t('account.current_password')"
                  type="password"
                  autocomplete="current-password"
                  dense
                  outlined
                />
                <q-input
                  v-model="form.newPassword"
                  :label="t('account.new_password')"
                  type="password"
                  autocomplete="new-password"
                  dense
                  outlined
                />
                <q-input
                  v-model="form.confirmNewPassword"
                  :label="t('account.confirm_password')"
                  type="password"
                  autocomplete="new-password"
                  dense
                  outlined
                />
                <div class="row justify-end">
                  <q-btn
                    type="submit"
                    unelevated
                    color="primary"
                    :label="t('account.change_password_button')"
                    :loading="isSubmitting"
                    :disable="isSubmitting"
                  />
                </div>
              </q-form>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import AuthService, { type ChangePasswordRequest } from '@/api/services/AuthService';
import PageLayout from '@/layouts/PageLayout.vue';
import { handleError } from '@/utils/error-handler';

type PasswordForm = {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
};

const { t } = useI18n();

const form = ref<PasswordForm>({
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
});
const isSubmitting = ref(false);

async function changePassword() {
  if (form.value.newPassword !== form.value.confirmNewPassword) {
    toast.error(t('account.password_mismatch'));
    return;
  }

  const payload: ChangePasswordRequest = {
    currentPassword: form.value.currentPassword,
    newPassword: form.value.newPassword,
  };

  isSubmitting.value = true;
  const response = await AuthService.changePassword(payload);
  isSubmitting.value = false;

  if (response.error) {
    handleError(response.error, t('account.password_change_failed'));
    return;
  }

  toast.success(t('account.password_changed'));
  form.value = {
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  };
}
</script>

<style scoped>
.text-body2 {
  color: rgba(0, 0, 0, 0.65);
}
</style>
