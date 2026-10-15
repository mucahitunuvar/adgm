import type { ApiClient } from './client';
import type { components } from './schema';

export type ContentItemSummaryResponse = components['schemas']['ContentItemSummaryResponse'];
export type PagedResultOfContentItemSummaryResponse = components['schemas']['PagedResultOfContentItemSummaryResponse'];
export type ContentItemDetailResponse = components['schemas']['ContentItemDetailResponse'];
export type ContentItemTranslationResponse = components['schemas']['ContentItemTranslationResponse'];
export type ContentItemSeoResponse = components['schemas']['ContentItemSeoResponse'];
export type ContentItemGalleryItemResponse = components['schemas']['ContentItemGalleryItemResponse'];
export type ContentItemAttachmentResponse = components['schemas']['ContentItemAttachmentResponse'];
export type CreateContentItemRequest = components['schemas']['CreateContentItemRequest'];
export type CreateContentItemResponse = components['schemas']['CreateContentItemResponse'];
export type UpdateContentItemRequest = components['schemas']['UpdateContentItemRequest'];
export type UpdateContentItemTranslationRequest = components['schemas']['UpdateContentItemTranslationRequest'];
export type SetContentItemCategoriesRequest = components['schemas']['SetContentItemCategoriesRequest'];
export type SetContentItemTranslationTagsRequest = components['schemas']['SetContentItemTranslationTagsRequest'];
export type SetContentItemGalleryRequest = components['schemas']['SetContentItemGalleryRequest'];
export type SetContentItemVideosRequest = components['schemas']['SetContentItemVideosRequest'];
export type SetContentItemAttachmentsRequest = components['schemas']['SetContentItemAttachmentsRequest'];
export type SetContentItemRelatedContentRequest = components['schemas']['SetContentItemRelatedContentRequest'];
export type SetContentItemParentRequest = components['schemas']['SetContentItemParentRequest'];
export type PublishContentItemRequest = components['schemas']['PublishContentItemRequest'];
export type ScheduleContentItemRequest = components['schemas']['ScheduleContentItemRequest'];
export type DuplicateContentItemResponse = components['schemas']['DuplicateContentItemResponse'];
export type ContentItemTrashSummaryResponse = components['schemas']['ContentItemTrashSummaryResponse'];
export type PagedResultOfContentItemTrashSummaryResponse = components['schemas']['PagedResultOfContentItemTrashSummaryResponse'];
export type ContentItemRevisionSummaryResponse = components['schemas']['ContentItemRevisionSummaryResponse'];
export type PagedResultOfContentItemRevisionSummaryResponse = components['schemas']['PagedResultOfContentItemRevisionSummaryResponse'];
export type ContentItemRevisionDetailResponse = components['schemas']['ContentItemRevisionDetailResponse'];
export type CompareContentItemRevisionsResponse = components['schemas']['CompareContentItemRevisionsResponse'];
export type RestoreContentItemRevisionRequest = components['schemas']['RestoreContentItemRevisionRequest'];
export type CreateContentPreviewLinkRequest = components['schemas']['CreateContentPreviewLinkRequest'];
export type CreateContentPreviewLinkResponse = components['schemas']['CreateContentPreviewLinkResponse'];

interface RowVersioned {
  rowVersion: string;
}

function basePath(id: string): string {
  return `/api/v1/admin/website/contents/${encodeURIComponent(id)}`;
}

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

/**
 * `GET /api/v1/admin/website/contents`. Backend bu filtreleri desteklemiyor: kategori, etiket,
 * tarih aralığı, açık sıralama (orderBy) - bkz. FRONTEND-ACIK-MADDELER.md §B. Sayfalamalı bir
 * sonuç üzerinde bunları istemci tarafında taklit etmek yanlış/eksik sonuç üretir, bu yüzden
 * yapılmadı.
 */
export function getContentItems(client: ApiClient, params: ListContentItemsParams = {}): Promise<PagedResultOfContentItemSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/contents', { query });
}

export function getContentItemById(client: ApiClient, id: string): Promise<ContentItemDetailResponse> {
  return client.request(basePath(id));
}

/** `POST /api/v1/admin/website/contents`. Yalnız varsayılan dilin çevirisini oluşturur. */
export function createContentItem(client: ApiClient, body: CreateContentItemRequest): Promise<CreateContentItemResponse> {
  return client.request('/api/v1/admin/website/contents', { method: 'POST', body });
}

/** `PUT .../{id}`. Sıra, öne çıkan, kapak/detay görseli - diğer tüm alanlar ayrı uçlardan değişir. */
export function updateContentItem(client: ApiClient, id: string, body: UpdateContentItemRequest): Promise<void> {
  return client.request(basePath(id), { method: 'PUT', body });
}

export function updateContentItemTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateContentItemTranslationRequest,
): Promise<void> {
  return client.request(`${basePath(id)}/translations/${encodeURIComponent(languageCode)}`, { method: 'PUT', body });
}

/** Varsayılan dilin çevirisi silinemez (backend invariant); arayüz bu seçeneği göstermez. */
export function deleteContentItemTranslation(client: ApiClient, id: string, languageCode: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(id)}/translations/${encodeURIComponent(languageCode)}`, { method: 'DELETE', body });
}

/** Dil-bağımsız (`ContentItem.SetCategories`, en fazla 10). */
export function setContentItemCategories(client: ApiClient, id: string, body: SetContentItemCategoriesRequest): Promise<void> {
  return client.request(`${basePath(id)}/categories`, { method: 'PUT', body });
}

/** Dil bazlı (`ContentItem.SetTranslationTags`) - kategorilerin tersine her çeviri kendi etiket kümesini taşır. */
export function setContentItemTranslationTags(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: SetContentItemTranslationTagsRequest,
): Promise<void> {
  return client.request(`${basePath(id)}/translations/${encodeURIComponent(languageCode)}/tags`, { method: 'PUT', body });
}

/** Tüm listeyi değiştirir (ekleme/çıkarma/sıralama ayrı uç değil, en fazla 100 öğe). */
export function setContentItemGallery(client: ApiClient, id: string, body: SetContentItemGalleryRequest): Promise<void> {
  return client.request(`${basePath(id)}/gallery`, { method: 'PUT', body });
}

/** Sıra, `videoIds` dizisinin kendi sırasıyla belirlenir (en fazla 20). */
export function setContentItemVideos(client: ApiClient, id: string, body: SetContentItemVideosRequest): Promise<void> {
  return client.request(`${basePath(id)}/videos`, { method: 'PUT', body });
}

/** Tüm listeyi değiştirir (en fazla 30 öğe). */
export function setContentItemAttachments(client: ApiClient, id: string, body: SetContentItemAttachmentsRequest): Promise<void> {
  return client.request(`${basePath(id)}/attachments`, { method: 'PUT', body });
}

/** En fazla 12 ilişkili içerik; kendine referans ve tekrar backend'de reddedilir. */
export function setContentItemRelatedContent(client: ApiClient, id: string, body: SetContentItemRelatedContentRequest): Promise<void> {
  return client.request(`${basePath(id)}/related`, { method: 'PUT', body });
}

/** `parentId: null` kök seviyeye taşır. Döngü/yol kontrolü backend'de. */
export function setContentItemParent(client: ApiClient, id: string, body: SetContentItemParentRequest): Promise<void> {
  return client.request(`${basePath(id)}/parent`, { method: 'PUT', body });
}

/** `publishAtUtc`/`unpublishAtUtc` boşsa anında yayınlanır. Üst içerik yayında olmalı. */
export function publishContentItem(client: ApiClient, id: string, body: PublishContentItemRequest): Promise<void> {
  return client.request(`${basePath(id)}/publish`, { method: 'POST', body });
}

/** Yalnız zaten `Published` durumdaki bir içeriğin zamanlamasını değiştirir. */
export function scheduleContentItem(client: ApiClient, id: string, body: ScheduleContentItemRequest): Promise<void> {
  return client.request(`${basePath(id)}/schedule`, { method: 'PUT', body });
}

/** Yayındaki alt içeriği varsa backend 409 döner. */
export function unpublishContentItem(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(id)}/unpublish`, { method: 'POST', body });
}

export function archiveContentItem(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(id)}/archive`, { method: 'POST', body });
}

/** `Archived` durumundan doğrudan `Published`'e dönmez - `Unpublished`'e döner, yeniden yayınlanmalıdır. */
export function unarchiveContentItem(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`${basePath(id)}/unarchive`, { method: 'POST', body });
}

/** Gövde yok (yalnız id); yanıt 201 ve yeni kopyanın id'sini döner. */
export function duplicateContentItem(client: ApiClient, id: string): Promise<DuplicateContentItemResponse> {
  return client.request(`${basePath(id)}/duplicate`, { method: 'POST' });
}

/** Çöp kutusuna taşır (kalıcı silme değil). Yayındaysa önce yayından kaldırılmalı. */
export function deleteContentItem(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(basePath(id), { method: 'DELETE', body });
}

export interface ListContentTrashParams {
  languageCode?: string;
  page?: number;
  pageSize?: number;
}

/** `ContentItem.TrashRetentionDays = 30` - `eligibleForPermanentDeletionAtUtc` bu süreye göre hesaplanır. */
export function getContentTrash(client: ApiClient, params: ListContentTrashParams = {}): Promise<PagedResultOfContentItemTrashSummaryResponse> {
  const query: Record<string, string | number | undefined> = { ...params };
  return client.request('/api/v1/admin/website/trash', { query });
}

export function restoreContentItem(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/trash/${encodeURIComponent(id)}/restore`, { method: 'POST' });
}

/** Geri dönüşü olmayan işlem; `WebsitePolicies.ContentPublish` gerektirir (diğer yaşam döngüsü uçlarından daha yüksek yetki). */
export function permanentlyDeleteContentItem(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/trash/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export interface ListContentItemRevisionsParams {
  page?: number;
  pageSize?: number;
}

export function getContentItemRevisions(
  client: ApiClient,
  id: string,
  params: ListContentItemRevisionsParams = {},
): Promise<PagedResultOfContentItemRevisionSummaryResponse> {
  const query: Record<string, number | undefined> = { ...params };
  return client.request(`${basePath(id)}/revisions`, { query });
}

export function getContentItemRevisionByNumber(client: ApiClient, id: string, revisionNumber: number): Promise<ContentItemRevisionDetailResponse> {
  return client.request(`${basePath(id)}/revisions/${revisionNumber}`);
}

/** `to`: bir revizyon numarası veya mevcut hali ("current") için literal "current". */
export function compareContentItemRevisions(
  client: ApiClient,
  id: string,
  params: { from: number; to: number | 'current'; lang?: string },
): Promise<CompareContentItemRevisionsResponse> {
  return client.request(`${basePath(id)}/revisions/compare`, { query: { from: params.from, to: params.to, lang: params.lang } });
}

export function restoreContentItemRevision(
  client: ApiClient,
  id: string,
  revisionNumber: number,
  body: RestoreContentItemRevisionRequest,
): Promise<void> {
  return client.request(`${basePath(id)}/revisions/${revisionNumber}/restore`, { method: 'POST', body });
}

/** Yayınlanmamış içeriği imzalı/süreli, anonim bir bağlantıyla önizlemeye açar (ADR-024 §4.5). */
export function createContentPreviewLink(
  client: ApiClient,
  id: string,
  body: CreateContentPreviewLinkRequest,
): Promise<CreateContentPreviewLinkResponse> {
  return client.request(`${basePath(id)}/preview-links`, { method: 'POST', body });
}
