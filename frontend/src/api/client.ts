import createClient from 'openapi-fetch';
import type { paths } from './generated/schema.d.ts';
import { useAuthStore } from '@/stores/auth-store.js';
import { toast } from 'vue3-toastify';

const baseUrl = process.env.VITE_API_URL || 'http://localhost:5097/';

const customFetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
  const request = new Request(input, init);
  const authStore = useAuthStore();

  if (authStore.accessToken) {
    request.headers.set('Authorization', `Bearer ${authStore.accessToken}`);
  }

  const fetchWithRetry = async (req: Request): Promise<Response> => {
    let response = await fetch(req.clone());

    if (response.status === 401 && !req.url.includes('/auth/refresh')) {
      if (authStore.refreshToken) {
        const newToken = await authStore.refreshAccessToken();
        if (newToken) {
          request.headers.set('Authorization', `Bearer ${newToken}`);
          const retriedRequest = new Request(request.clone(), { headers: request.headers });
          response = await fetch(retriedRequest);
        }
      }
    }

    if (response.status === 204 || response.headers.get('content-length') === '0') {
      return response;
    }

    return response;
  };

  try {
    return await fetchWithRetry(request);
  } catch (error) {
    toast.error('An error occurred. Please try again later.');
    throw error;
  }
};

const client = createClient<paths>({
  baseUrl: baseUrl,
  fetch: customFetch
});

export { baseUrl, client, customFetch };
