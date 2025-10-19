import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type LoginRequest = paths['/auth/login']['post']['requestBody']['content']['application/json'];
export type RegisterRequest = paths['/auth/register']['post']['requestBody']['content']['application/json'];

class AuthService {
  async login(body: LoginRequest) {
    return await client.POST('/auth/login', { body });
  }

  async loginByGoogle(token: string) {
    return await client.POST('/auth/google', { body: { googleToken: token } });
  }

  async refreshToken(refreshToken: string) {
    return await client.POST('/auth/refresh', { body: { refreshToken: refreshToken } });
  }

  async register(body: RegisterRequest) {
    return await client.POST('/auth/register', { body });
  }

  async updatePassword(oldPassword: string, newPassword: string) {
    return await client.PUT('/auth/password', { body: { oldPassword, newPassword } });
  }

  async updateEmail(email: string) {
    return await client.PUT('/auth/email', { body: { newEmail: email } });
  }
}

export default new AuthService();
