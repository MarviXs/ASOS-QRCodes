import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ScanRecordsQueryParams = paths['/scan-records']['get']['parameters']['query'];
export type ScanRecordsResponse = paths['/scan-records']['get']['responses']['200']['content']['application/json'];

class ScanRecordService {
  async getScanRecords(queryParams: ScanRecordsQueryParams) {
    return await client.GET('/scan-records', { params: { query: queryParams } });
  }
}

export default new ScanRecordService();
