import { boot } from 'quasar/wrappers';
import { createI18n } from 'vue-i18n';
import messages from '@intlify/unplugin-vue-i18n/messages';
import type { QuasarLanguage } from 'quasar';
import { Quasar } from 'quasar';

const langList = import.meta.glob('../../node_modules/quasar/lang/(en-US|sk).mjs');

export default boot(({ app }) => {
  const i18n = createI18n({
    legacy: false,
    locale: localStorage.getItem('locale') ?? 'en',
    messages,
  });

  let langIso = i18n.global.locale.value;
  if (langIso === 'en') langIso = 'en-US';
  try {
    langList[`../../node_modules/quasar/lang/${langIso}.mjs`]().then((lang: unknown) => {
      const typedLang = lang as { default: QuasarLanguage };
      Quasar.lang.set(typedLang.default);
    });
  } catch {
    console.error(`Language file for "${langIso}" not found`);
  }

  // Set i18n instance on app
  app.use(i18n);
});
