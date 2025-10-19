import { boot } from 'quasar/wrappers';
import Vue3Toasity, { type ToastContainerOptions } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';

export default boot(async ({ app }) => {
  const options: ToastContainerOptions = {
    autoClose: 3000,
    clearOnUrlChange: false,
  };

  app.use(Vue3Toasity, options);
});
