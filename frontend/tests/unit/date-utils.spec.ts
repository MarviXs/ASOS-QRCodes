import { describe, expect, it, vi } from 'vitest';

vi.mock('date-fns', () => ({
  formatDistanceToNowStrict: vi.fn(),
}));

vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));

import { formatDistanceToNowStrict } from 'date-fns';
import { enUS, sk } from 'date-fns/locale';
import { useI18n } from 'vue-i18n';

import { formatTimeToDistance } from '@/utils/date-utils';

const mockedFormatDistance = formatDistanceToNowStrict as unknown as ReturnType<typeof vi.fn>;
const mockedUseI18n = useI18n as unknown as ReturnType<typeof vi.fn>;

describe('formatTimeToDistance', () => {
  it('returns an empty string when time is falsy', () => {
    const result = formatTimeToDistance('');

    expect(result).toBe('');
    expect(mockedFormatDistance).not.toHaveBeenCalled();
  });

  it('uses Slovak locale when the active locale is sk', () => {
    mockedUseI18n.mockReturnValue({ locale: { value: 'sk' } });
    mockedFormatDistance.mockReturnValue('slovak-distance');

    const result = formatTimeToDistance('2024-01-01T00:00:00Z');

    expect(mockedFormatDistance).toHaveBeenCalledWith('2024-01-01T00:00:00Z', {
      addSuffix: true,
      locale: sk,
    });
    expect(result).toBe('slovak-distance');
  });

  it('falls back to the English locale for other language codes', () => {
    mockedUseI18n.mockReturnValue({ locale: { value: 'en' } });
    mockedFormatDistance.mockReturnValue('english-distance');

    const result = formatTimeToDistance('2024-01-01T00:00:00Z');

    expect(mockedFormatDistance).toHaveBeenCalledWith('2024-01-01T00:00:00Z', {
      addSuffix: true,
      locale: enUS,
    });
    expect(result).toBe('english-distance');
  });
});
