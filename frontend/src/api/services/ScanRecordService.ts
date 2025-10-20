import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

type GeneratedScanRecordsQuery = paths['/scan-records']['get']['parameters']['query'];
type ScanAnalyticsQuery = paths['/scan-records/analytics']['get']['parameters']['query'];

export type ScanRecordsQueryParams = GeneratedScanRecordsQuery & {
  StartDate?: string;
  EndDate?: string;
};
export type ScanRecordsResponse =
  paths['/scan-records']['get']['responses']['200']['content']['application/json'];
export type ScanAnalyticsResponse =
  paths['/scan-records/analytics']['get']['responses']['200']['content']['application/json'];

class ScanRecordService {
  async getScanRecords(queryParams: ScanRecordsQueryParams) {
    return await client.GET('/scan-records', { params: { query: queryParams } });
  }

  async getScanAnalytics(queryParams: ScanAnalyticsQuery) {
    return await client.GET('/scan-records/analytics', { params: { query: queryParams } });
  }
}

export default new ScanRecordService();
