<template>
  <PageLayout :breadcrumbs="[{ label: t('global.account'), to: '/account' }]">
    <template #default>
      <div class="row justify-center">
        <div class="col-12">
          <div class="row items-stretch q-col-gutter-xl">
            <div class="col-12 col-lg-5">
              <div class="account-copy">
                <div class="text-h6 text-weight-semibold">
                  {{ t('account.password_title') }}
                </div>
                <p class="text-body2 text-weight-regular q-mt-sm">
                  {{ t('account.password_description') }}
                </p>
              </div>
            </div>
            <div class="col-12 col-lg-7">
              <q-card class="shadow account-card">
                <q-card-section class="password-card-section">
                  <q-form class="q-gutter-xl" @submit.prevent="changePassword">
                    <q-input
                      v-model="form.currentPassword"
                      :label="t('account.current_password')"
                      :type="showCurrentPassword ? 'text' : 'password'"
                      autocomplete="current-password"
                      class="account-password-input"
                    >
                      <template #append>
                        <q-icon
                          :name="showCurrentPassword ? mdiEye : mdiEyeOff"
                          class="cursor-pointer"
                          @click="showCurrentPassword = !showCurrentPassword"
                        />
                      </template>
                    </q-input>
                    <q-input
                      v-model="form.newPassword"
                      :label="t('account.new_password')"
                      :type="showNewPassword ? 'text' : 'password'"
                      autocomplete="new-password"
                      class="account-password-input"
                    >
                      <template #append>
                        <q-icon
                          :name="showNewPassword ? mdiEye : mdiEyeOff"
                          class="cursor-pointer"
                          @click="showNewPassword = !showNewPassword"
                        />
                      </template>
                    </q-input>
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
import { mdiEye, mdiEyeOff } from '@quasar/extras/mdi-v7';

type PasswordForm = {
  currentPassword: string;
  newPassword: string;
};

const { t } = useI18n();

const form = ref<PasswordForm>({
  currentPassword: '',
  newPassword: '',
});
const isSubmitting = ref(false);
const showCurrentPassword = ref(false);
const showNewPassword = ref(false);

async function changePassword() {
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
  };
}
</script>

<style scoped>
.text-body2 {
  color: rgba(0, 0, 0, 0.65);
}

.account-copy {
  padding-top: 2rem;
  padding-bottom: 2rem;
}

.account-card {
  border-radius: 4px;
  min-height: 280px;
}

.password-card-section {
  padding: 2.5rem 2.25rem;
}

.account-password-input .q-field__control {
  border-bottom: 1px solid rgba(0, 0, 0, 0.18);
  border-radius: 0;
  padding-bottom: 0.2rem;
}

.account-password-input .q-field__control:focus-within,
.account-password-input.q-field--focused .q-field__control {
  border-bottom-color: var(--q-color-primary);
}

.account-password-input .q-field__native {
  padding: 0;
}
</style>
