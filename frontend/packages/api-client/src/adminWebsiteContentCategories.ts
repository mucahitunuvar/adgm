import type { ApiClient } from './client';
import type { components } from './schema';

export type ContentCategoryTreeItemResponse = components['schemas']['ContentCategoryTreeItemResponse'];
export type ContentCategoryDetailResponse = components['schemas']['ContentCategoryDetailResponse'];
export type CreateContentCategoryRequest = components['schemas']['CreateContentCategoryRequest'];
export type CreateContentCategoryResponse = components['schemas']['CreateContentCategoryResponse'];
export type UpdateContentCategoryRequest = components['schemas']['UpdateContentCategoryRequest'];
export type UpdateContentCategoryTranslationRequest = components['schemas']['UpdateContentCategoryTranslationRequest'];

interface RowVersioned {
  rowVersion: string;
}

function basePath(typeId: string): string {
  return `/api/v1/admin/website/content-types/${encodeURIComponent(typeId)}/categories`;
}

/** `GET .../categories`. Ağaç olarak döner (üst/alt yapı backend'de çözülmüş). */
export function getContentCategoriesByType(client: ApiClient, typeId: string): Promise<ContentCategoryTreeItemResponse[]> {
  return client.request(basePath(typeId));
}

/** `GET .../categories/{id}`. Çeviri/SEO alanlarını (ad/slug + tam SEO kümesi) okur. */
export function getContentCategoryById(client: ApiClient, typeId: string, id: string): Promise<ContentCategoryDetailResponse> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}`);
}

export function createContentCategory(
  client: ApiClient,
  typeId: string,
  body: CreateContentCategoryRequest,
): Promise<CreateContentCategoryResponse> {
  return client.request(basePath(typeId), { method: 'POST', body });
}

/** Yalnız sıra (`sortOrder`) güncellenir; ad/slug çeviri ucundan değişir. */
export function updateContentCategory(
  client: ApiClient,
  typeId: string,
  id: string,
  body: UpdateContentCategoryRequest,
): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

/** İçerik ataması olan veya alt kategorisi olan bir kategori silinemez (backend 409 döner). */
export function deleteContentCategory(client: ApiClient, typeId: string, id: string): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function activateContentCategory(client: ApiClient, typeId: string, id: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}/activate`, { method: 'POST', body });
}

export function deactivateContentCategory(client: ApiClient, typeId: string, id: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}/deactivate`, { method: 'POST', body });
}

export function updateContentCategoryTranslation(
  client: ApiClient,
  typeId: string,
  id: string,
  languageCode: string,
  body: UpdateContentCategoryTranslationRequest,
): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'PUT',
    body,
  });
}

export function deleteContentCategoryTranslation(
  client: ApiClient,
  typeId: string,
  id: string,
  languageCode: string,
  body: RowVersioned,
): Promise<void> {
  return client.request(`${basePath(typeId)}/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'DELETE',
    body,
  });
}
