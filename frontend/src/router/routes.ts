import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: '/qr-codes',
      },
      {
        path: '/qr-codes',
        component: () => import('pages/qr-codes/QRCodesListPage.vue'),
      },
      {
        path: '/qr-codes/create',
        component: () => import('pages/qr-codes/QRCodeCreatePage.vue'),
      },
      {
        path: '/qr-codes/:id/edit',
        component: () => import('pages/qr-codes/QRCodeEditPage.vue'),
      }
    ]
  },
  {
    path: '/login',
    component: () => import('pages/auth/LoginPage.vue'),
  },
  {
    path: '/register',
    component: () => import('pages/auth/RegisterPage.vue'),
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
