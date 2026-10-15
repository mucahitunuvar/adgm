import type { ApiClient } from './client';
import type { components } from './schema';

export type TagResponse = components['schemas']['TagResponse'];
export type PagedResultOfTagResponse = components['schemas']['PagedResultOfTagResponse'];
export type UpdateTagRequest = components['schemas']['UpdateTagRequest'];

export interface ListTagsParams {
  languageCode?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/tags`. Yalnız Admin. */
export function listTags(client: ApiClient, params: ListTagsParams = {}): Promise<PagedResultOfTagResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/tags', { query });
}

/** `PUT /api/v1/admin/website/tags/{id}`. Yalnız ad değişir; slug backend'de yeniden üretilir. */
export function updateTag(client: ApiClient, id: string, body: UpdateTagRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/tags/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

/** Kullanımda olsa bile silinebilir (içerikten etiket bağı kopar); onay diyaloğu bunu açıklar. */
export function deleteTag(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/tags/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

/** `sourceId`'nin tüm içerik atamalarını `targetId`'ye taşır, `sourceId`'yi siler. */
export function mergeTag(client: ApiClient, sourceId: string, targetId: string): Promise<void> {
  return client.request(
    `/api/v1/admin/website/tags/${encodeURIComponent(sourceId)}/merge-into/${encodeURIComponent(targetId)}`,
    { method: 'POST' },
  );
}
