import { boot } from 'quasar/wrappers';
import vue3GoogleLogin from 'vue3-google-login';

export default boot(async ({ app }) => {
  const config = {
    clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
  };

  app.use(vue3GoogleLogin, config);
});
