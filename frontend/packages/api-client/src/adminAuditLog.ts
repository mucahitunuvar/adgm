import type { ApiClient } from './client';
import type { components } from './schema';

export type AdminGetAuditLogResponse = components['schemas']['AdminGetAuditLogResponse'];
export type AdminAuditLogItemResponse = components['schemas']['AdminAuditLogItemResponse'];

export interface ListAuditLogParams {
  targetUserId?: string;
  actionType?: string;
  fromUtc?: string;
  toUtc?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/auth/admin/audit-log`. Yalnız Admin, salt okunur. */
export function listAuditLog(client: ApiClient, params: ListAuditLogParams = {}): Promise<AdminGetAuditLogResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/auth/admin/audit-log', { query });
}
