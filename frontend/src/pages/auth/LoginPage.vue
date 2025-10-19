<template>
  <div class="auth-bg">
    <div class="auth-container shadow">
      <h1>{{ t('auth.login.label') }}</h1>
      <div class="q-mt-lg">
        <q-form>
          <q-input
            ref="nameRef"
            v-model="userLogin.email"
            :label="t('account.email')"
            type="text"
            lazy-rules
            :rules="nameRules"
          >
            <template #prepend>
              <q-icon :name="mdiAccount" />
            </template>
          </q-input>
          <q-input
            ref="passwordRef"
            v-model="userLogin.password"
            :label="t('account.password')"
            :type="isPwd ? 'password' : 'text'"
            lazy-rules
            :rules="passwordRules"
          >
            <template #prepend>
              <q-icon :name="mdiLock" />
            </template>
            <template #append>
              <q-icon :name="isPwd ? mdiEyeOff : mdiEye" class="cursor-pointer" @click="isPwd = !isPwd" />
            </template>
          </q-input>
          <q-btn
            class="q-my-md full-width"
            color="primary"
            :label="t('auth.login.login_btn')"
            type="submit"
            size="1rem"
            no-caps
            unelevated
            :loading="isSubmitting"
            @click.prevent="login"
          />
        </q-form>
        <div class="column items-center links">
          <GoogleLoginBtn v-model:loading="isSubmitting" />
          <div class="q-md-md q-mt-lg">
            <span>{{ t('auth.login.no_account') }}</span>
            <router-link to="/register" class="q-ml-sm">
              {{ t('auth.login.sign_up') }}
            </router-link>
          </div>
          <language-select />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { toast } from 'vue3-toastify';
import { QInput } from 'quasar';
import { useAuthStore } from '@/stores/auth-store';
import { isFormValid } from '@/utils/form-validation';
import LanguageSelect from '@/components/core/LanguageSelect.vue';
import { useI18n } from 'vue-i18n';
import { mdiAccount, mdiEye, mdiEyeOff, mdiLock } from '@quasar/extras/mdi-v7';
import GoogleLoginBtn from '@/components/account/GoogleLoginBtn.vue';
import { handleError } from '@/utils/error-handler';
import type { LoginRequest } from '@/api/services/AuthService';

const { t } = useI18n();

const router = useRouter();
const authStore = useAuthStore();

const userLogin = ref<LoginRequest>({
  email: '',
  password: '',
});

const isPwd = ref(true);
const isSubmitting = ref(false);

//Form validation
const nameRef = ref<QInput>();
const passwordRef = ref<QInput>();

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const passwordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

async function login() {
  const form = [nameRef.value, passwordRef.value];
  if (!isFormValid(form)) return;

  isSubmitting.value = true;
  const { error } = await authStore.login(userLogin.value);
  isSubmitting.value = false;

  if (error) {
    handleError(error, t('auth.login.toasts.login_failed'));
    return;
  }

  toast.success(t('auth.login.toasts.login_success'));
  router.push('/');
}
</script>

<style lang="scss" scoped>
@import '@/css/auth.scss';
</style>
