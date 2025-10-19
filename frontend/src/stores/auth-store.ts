import { defineStore } from 'pinia';
import { useStorage } from '@vueuse/core';
import type { User } from '@/models/User';
import AuthService from '@/api/services/AuthService';
import { computed } from 'vue';
import { jwtDecode } from 'jwt-decode';
import { Role } from '@/models/Role';
import { useStoreRouter } from '@/composables/useStoreRouter';
import type { JwtPayload } from '@/models/Tokens';
import type { LoginRequest } from '@/api/services/AuthService';

export const useAuthStore = defineStore('authStore', () => {
  const router = useStoreRouter();

  const accessToken = useStorage('accessToken', '');
  const refreshToken = useStorage('refreshToken', '');
  const isAuthenticated = computed(() => !!accessToken.value);

  async function login(user: LoginRequest) {
    const res = await AuthService.login(user);

    clearJwt();
    if (res.data) {
      accessToken.value = res.data.accessToken;
      refreshToken.value = res.data.refreshToken;
    }

    return res;
  }

  async function loginByGoogle(token: string) {
    clearJwt();
    const { data, error } = await AuthService.loginByGoogle(token);

    if (data) {
      accessToken.value = data.accessToken;
      refreshToken.value = data.refreshToken;
    }

    return { data, error };
  }

  async function refreshAccessToken() {
    const res = await AuthService.refreshToken(refreshToken.value);
    if (res.data) {
      accessToken.value = res.data.accessToken;
      return res.data.accessToken;
    }
    return null;
  }

  function clearJwt() {
    accessToken.value = '';
    refreshToken.value = '';
  }

  function logout() {
    clearJwt();
    router.push('/login');
  }

  const decodeJwt = () => {
    if (!accessToken.value) return null;
    try {
      const decodedToken = jwtDecode<JwtPayload>(accessToken.value);
      return decodedToken;
    } catch {
      logout();
      console.error('Invalid JWT');
      return null;
    }
  };

  const user = computed(() => {
    const decodedToken = decodeJwt();
    if (!decodedToken) return null;

    const id = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const email = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];

    const user: User = {
      id,
      email,
    };

    return user;
  });

  const isAdmin = computed(() => {
    const decodedToken = decodeJwt();
    const roles = decodedToken?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    return Array.isArray(roles) ? roles.includes(Role.ADMIN) : roles === Role.ADMIN;
  });

  return {
    accessToken,
    refreshToken,
    login,
    logout,
    isAuthenticated,
    user,
    isAdmin,
    clearJwt,
    loginByGoogle,
    refreshAccessToken,
  };
});
