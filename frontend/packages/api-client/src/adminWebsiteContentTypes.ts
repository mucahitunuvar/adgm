import type { ApiClient } from './client';
import type { components } from './schema';

export type ContentTypeSummaryResponse = components['schemas']['ContentTypeSummaryResponse'];
export type ContentTypeDetailResponse = components['schemas']['ContentTypeDetailResponse'];
export type ContentTypeTranslationResponse = components['schemas']['ContentTypeTranslationResponse'];
export type CreateContentTypeRequest = components['schemas']['CreateContentTypeRequest'];
export type CreateContentTypeResponse = components['schemas']['CreateContentTypeResponse'];
export type UpdateContentTypeRequest = components['schemas']['UpdateContentTypeRequest'];
export type UpdateContentTypeTranslationRequest = components['schemas']['UpdateContentTypeTranslationRequest'];

interface RowVersioned {
  rowVersion: string;
}

/** `GET /api/v1/admin/website/content-types`. Sayfalama yok (yönetilebilir sayıda tip beklenir). */
export function getContentTypes(client: ApiClient): Promise<ContentTypeSummaryResponse[]> {
  return client.request('/api/v1/admin/website/content-types');
}

/** `GET /api/v1/admin/website/content-types/{id}`. */
export function getContentTypeById(client: ApiClient, id: string): Promise<ContentTypeDetailResponse> {
  return client.request(`/api/v1/admin/website/content-types/${encodeURIComponent(id)}`);
}

/** `POST /api/v1/admin/website/content-types`. Varsayılan dilin çevirisini de birlikte oluşturur. */
export function createContentType(client: ApiClient, body: CreateContentTypeRequest): Promise<CreateContentTypeResponse> {
  return client.request('/api/v1/admin/website/content-types', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/content-types/{id}`. `key` değiştirilemez (rota/şema kararlılığı). */
export function updateContentType(client: ApiClient, id: string, body: UpdateContentTypeRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/content-types/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

export function activateContentType(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/content-types/${encodeURIComponent(id)}/activate`, { method: 'POST', body });
}

export function deactivateContentType(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/content-types/${encodeURIComponent(id)}/deactivate`, { method: 'POST', body });
}

export function updateContentTypeTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateContentTypeTranslationRequest,
): Promise<void> {
  return client.request(
    `/api/v1/admin/website/content-types/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`,
    { method: 'PUT', body },
  );
}

/** Varsayılan dilin çevirisi silinemez (backend invariant); arayüz bu seçeneği göstermez. */
export function deleteContentTypeTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: RowVersioned,
): Promise<void> {
  return client.request(
    `/api/v1/admin/website/content-types/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`,
    { method: 'DELETE', body },
  );
}
