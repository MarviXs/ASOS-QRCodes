const DEFAULT_API_URL = 'http://localhost:5097';

function getApiUrlBase(): string {
  const fromEnv = import.meta.env.VITE_API_URL as string | undefined;
  return (fromEnv && fromEnv.trim().length > 0 ? fromEnv : DEFAULT_API_URL).replace(/\/+$/, '');
}

export function buildScanUrl(shortCode: string): string {
  const base = getApiUrlBase();
  return `${base}/scan/${shortCode}`;
}
