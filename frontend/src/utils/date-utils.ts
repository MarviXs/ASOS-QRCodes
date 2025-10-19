import { formatDistanceToNowStrict } from 'date-fns';
import { enUS, sk } from 'date-fns/locale';
import { useI18n } from 'vue-i18n';

const formatTimeToDistance = (time: string) => {
  if (!time) return '';
  let locale;
  const i18n = useI18n();
  const localeCode = i18n.locale.value;

  if (localeCode === 'sk') {
    locale = sk;
  } else {
    locale = enUS;
  }

  return formatDistanceToNowStrict(time, { addSuffix: true, locale });
};

export { formatTimeToDistance };
