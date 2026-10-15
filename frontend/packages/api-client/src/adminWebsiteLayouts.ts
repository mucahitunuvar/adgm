import type { ApiClient } from './client';
import type { components } from './schema';

export type LayoutBlockResponse = components['schemas']['LayoutBlockResponse'];
export type LayoutBlockInput = components['schemas']['LayoutBlockInput'];
export type GetHomeLayoutResponse = components['schemas']['GetHomeLayoutResponse'];
export type GetHomeLayoutPreviewResponse = components['schemas']['GetHomeLayoutPreviewResponse'];
export type PublicLayoutBlockResponse = components['schemas']['PublicLayoutBlockResponse'];
export type GetContentLayoutResponse = components['schemas']['GetContentLayoutResponse'];
export type ReplaceHomeDraftBlocksRequest = components['schemas']['ReplaceHomeDraftBlocksRequest'];
export type ReplaceContentDraftBlocksRequest = components['schemas']['ReplaceContentDraftBlocksRequest'];

export function getHomeLayout(client: ApiClient): Promise<GetHomeLayoutResponse> {
  return client.request('/api/v1/admin/website/layouts/home');
}

/** Taslak bloklardan, public `/home` ile AYNI biçimde (çözümlenmiş veriyle) önizleme - yayınlamadan. */
export function getHomeLayoutPreview(client: ApiClient, lang?: string): Promise<GetHomeLayoutPreviewResponse> {
  return client.request('/api/v1/admin/website/layouts/home/preview', { query: { lang } });
}

/** Taslak blok listesinin TAMAMINI değiştirir (ekleme/çıkarma/sıralama ayrı uç değil). */
export function replaceHomeDraftBlocks(client: ApiClient, body: ReplaceHomeDraftBlocksRequest): Promise<void> {
  return client.request('/api/v1/admin/website/layouts/home/draft', { method: 'PUT', body });
}

export function publishHomeLayout(client: ApiClient, rowVersion: string): Promise<void> {
  return client.request('/api/v1/admin/website/layouts/home/publish', { method: 'POST', body: { rowVersion } });
}

export function discardHomeLayoutDraft(client: ApiClient, rowVersion: string): Promise<void> {
  return client.request('/api/v1/admin/website/layouts/home/discard-draft', { method: 'POST', body: { rowVersion } });
}

/** İçerik için düzen henüz oluşturulmamışsa boş taslak (`rowVersion: []`) döner - "henüz yok" sentinel'i. */
export function getContentLayout(client: ApiClient, contentItemId: string): Promise<GetContentLayoutResponse> {
  return client.request(`/api/v1/admin/website/layouts/content/${encodeURIComponent(contentItemId)}`);
}

/** `rowVersion` boşsa (içerik için düzen hiç oluşturulmamışsa) backend eşzamanlılık kontrolünü atlar. */
export function replaceContentDraftBlocks(
  client: ApiClient,
  contentItemId: string,
  body: ReplaceContentDraftBlocksRequest,
): Promise<void> {
  return client.request(`/api/v1/admin/website/layouts/content/${encodeURIComponent(contentItemId)}/draft`, { method: 'PUT', body });
}

export function publishContentLayout(client: ApiClient, contentItemId: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/layouts/content/${encodeURIComponent(contentItemId)}/publish`, {
    method: 'POST',
    body: { rowVersion },
  });
}

export function discardContentLayoutDraft(client: ApiClient, contentItemId: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/layouts/content/${encodeURIComponent(contentItemId)}/discard-draft`, {
    method: 'POST',
    body: { rowVersion },
  });
}
