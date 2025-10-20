import VueApexCharts from "vue3-apexcharts";
import { boot } from 'quasar/wrappers';

export default boot(async ({ app }) => {
  app.use(VueApexCharts);
});
