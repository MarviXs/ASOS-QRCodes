import { describe, expect, it, vi } from 'vitest';

import { buildScanUrl } from '@/utils/qr-url';

describe('buildScanUrl', () => {
  it('falls back to default API url when env is not provided', () => {
    const url = buildScanUrl('abc123');
    expect(url).toBe('http://localhost:5097/scan/abc123');
  });

  it('uses configured API url and trims trailing slashes', () => {
    vi.stubEnv('VITE_API_URL', 'https://api.example.com///');

    const url = buildScanUrl('xyz');

    expect(url).toBe('https://api.example.com/scan/xyz');
  });

  it('ignores blank API url values', () => {
    vi.stubEnv('VITE_API_URL', '   ');

    const url = buildScanUrl('qwerty');

    expect(url).toBe('http://localhost:5097/scan/qwerty');
  });
});
