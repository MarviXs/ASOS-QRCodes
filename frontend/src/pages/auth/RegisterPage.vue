<template>
  <div class="auth-bg">
    <div class="auth-container shadow">
      <h1>{{ t('auth.register.label') }}</h1>
      <div class="q-mt-md">
        <q-form>
          <q-input
            ref="mailRef"
            v-model="userRegister.email"
            :label="t('account.email')"
            type="email"
            lazy-rules
            :rules="mailRules"
          >
            <template #prepend>
              <q-icon :name="mdiEmail" />
            </template>
          </q-input>
          <q-input
            ref="passwordRef"
            v-model="userRegister.password"
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
            :label="t('auth.register.register_btn')"
            type="submit"
            size="1rem"
            no-caps
            unelevated
            :loading="isSubmitting"
            @click.prevent="register"
          />
        </q-form>
        <div class="column items-center links">
          <GoogleLoginBtn v-model:loading="isSubmitting" />
          <div class="q-md-md q-mt-lg">
            <span>{{ t('auth.register.have_account') }}</span>
            <router-link to="/login" class="q-ml-sm">
              {{ t('auth.register.login') }}
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
import AuthService from '@/api/services/AuthService';
import { useRouter } from 'vue-router';
import { toast } from 'vue3-toastify';
import { QInput } from 'quasar';
import { handleError } from '@/utils/error-handler';
import { isFormValid } from '@/utils/form-validation';
import LanguageSelect from '@/components/core/LanguageSelect.vue';
import { useI18n } from 'vue-i18n';
import { mdiEmail, mdiEye, mdiEyeOff, mdiLock } from '@quasar/extras/mdi-v7';
import GoogleLoginBtn from '@/components/account/GoogleLoginBtn.vue';
import type { RegisterRequest } from '@/api/services/AuthService';

const { t } = useI18n();
const router = useRouter();

const userRegister = ref<RegisterRequest>({
  email: '',
  password: '',
});
const isPwd = ref(true);
const isSubmitting = ref(false);

//Form validation
const nicknameRef = ref<QInput>();
const mailRef = ref<QInput>();
const passwordRef = ref<QInput>();

const mailRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => {
    const emailRegex = /^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$/;
    return emailRegex.test(val) || t('auth.rules.email_invalid');
  },
];
const passwordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

async function register() {
  const form = [nicknameRef.value, mailRef.value, passwordRef.value];
  if (!isFormValid(form)) return;

  isSubmitting.value = true;
  const { error } = await AuthService.register(userRegister.value);
  isSubmitting.value = false;

  if (error) {
    handleError(error, t('auth.register.toasts.register_failed'));
    return;
  }

  toast.success(t('auth.register.toasts.register_success'));
  router.push('/');
}
</script>

<style lang="scss" scoped>
@import '@/css/auth.scss';
</style>
