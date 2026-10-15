import type { ApiClient } from './client';
import type { components } from './schema';

export type ContentItemSummaryResponse = components['schemas']['ContentItemSummaryResponse'];
export type PagedResultOfContentItemSummaryResponse = components['schemas']['PagedResultOfContentItemSummaryResponse'];
export type ContentItemDetailResponse = components['schemas']['ContentItemDetailResponse'];
export type ContentItemTranslationResponse = components['schemas']['ContentItemTranslationResponse'];
export type ContentItemSeoResponse = components['schemas']['ContentItemSeoResponse'];
export type CreateContentItemRequest = components['schemas']['CreateContentItemRequest'];
export type CreateContentItemResponse = components['schemas']['CreateContentItemResponse'];
export type UpdateContentItemTranslationRequest = components['schemas']['UpdateContentItemTranslationRequest'];

export interface ListContentItemsParams {
  contentTypeId?: string;
  status?: string;
  languageCode?: string;
  search?: string;
  isFeatured?: boolean;
  parentId?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/contents`. */
export function getContentItems(client: ApiClient, params: ListContentItemsParams = {}): Promise<PagedResultOfContentItemSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/contents', { query });
}

/** `GET /api/v1/admin/website/contents/{id}`. */
export function getContentItemById(client: ApiClient, id: string): Promise<ContentItemDetailResponse> {
  return client.request(`/api/v1/admin/website/contents/${encodeURIComponent(id)}`);
}

/** `POST /api/v1/admin/website/contents`. Yalnız varsayılan dilin çevirisini oluşturur. */
export function createContentItem(client: ApiClient, body: CreateContentItemRequest): Promise<CreateContentItemResponse> {
  return client.request('/api/v1/admin/website/contents', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/contents/{id}/translations/{languageCode}`. */
export function updateContentItemTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateContentItemTranslationRequest,
): Promise<void> {
  return client.request(
    `/api/v1/admin/website/contents/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`,
    { method: 'PUT', body },
  );
}
