import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type QRCodeQueryParams = paths['/qr-codes']['get']['parameters']['query'];
export type QRCodeResponse = paths['/qr-codes']['get']['responses']['200']['content']['application/json'];
export type CreateQRCodeRequest = paths['/qr-codes']['post']['requestBody']['content']['application/json'];
export type UpdateQRCodeRequest = paths['/qr-codes/{id}']['put']['requestBody']['content']['application/json'];

class QRCodeService {
  async getQRCodes(queryParams: QRCodeQueryParams) {
    return await client.GET('/qr-codes', { params: { query: queryParams } });
  }
  async getQRCode(qrCodeId: string) {
    return await client.GET('/qr-codes/{id}', { params: { path: { id: qrCodeId } } });
  }

  async createQRCode(body: CreateQRCodeRequest) {
    return await client.POST('/qr-codes', { body });
  }

  async updateQRCode(qrCodeId: string, body: UpdateQRCodeRequest) {
    return await client.PUT('/qr-codes/{id}', { body, params: { path: { id: qrCodeId } } });
  }

  async deleteQRCode(qrCodeId: string) {
    return await client.DELETE('/qr-codes/{id}', { params: { path: { id: qrCodeId } } });
  }
}

export default new QRCodeService();
