import type { ApiClient } from './client';
import type { components } from './schema';

export type MediaAssetSummaryResponse = components['schemas']['MediaAssetSummaryResponse'];
export type PagedResultOfMediaAssetSummaryResponse = components['schemas']['PagedResultOfMediaAssetSummaryResponse'];
export type MediaAssetDetailResponse = components['schemas']['MediaAssetDetailResponse'];
export type GetMediaAssetFoldersResponse = components['schemas']['GetMediaAssetFoldersResponse'];
export type UploadMediaAssetResponse = components['schemas']['UploadMediaAssetResponse'];
export type UpdateMediaAssetRequest = components['schemas']['UpdateMediaAssetRequest'];

export interface ListMediaAssetsParams {
  kind?: string;
  folder?: string;
  search?: string;
  missingAltText?: boolean;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/media`. */
export function getMediaAssets(client: ApiClient, params: ListMediaAssetsParams = {}): Promise<PagedResultOfMediaAssetSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/media', { query });
}

/** `GET /api/v1/admin/website/media/{id}`. */
export function getMediaAssetById(client: ApiClient, id: string): Promise<MediaAssetDetailResponse> {
  return client.request(`/api/v1/admin/website/media/${encodeURIComponent(id)}`);
}

/** `GET /api/v1/admin/website/media/folders`. */
export function getMediaAssetFolders(client: ApiClient): Promise<GetMediaAssetFoldersResponse> {
  return client.request('/api/v1/admin/website/media/folders');
}

export interface UploadMediaAssetParams {
  file: File;
  folder?: string;
  altText?: string;
  caption?: string;
}

/** `POST /api/v1/admin/website/media` (multipart/form-data, `IFormFile` bağlaması). */
export function uploadMediaAsset(client: ApiClient, params: UploadMediaAssetParams): Promise<UploadMediaAssetResponse> {
  const body = new FormData();
  body.set('file', params.file);
  if (params.folder) body.set('folder', params.folder);
  if (params.altText) body.set('altText', params.altText);
  if (params.caption) body.set('caption', params.caption);
  return client.request('/api/v1/admin/website/media', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/media/{id}`. */
export function updateMediaAsset(client: ApiClient, id: string, body: UpdateMediaAssetRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/media/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

/** `DELETE /api/v1/admin/website/media/{id}`. Kullanımdaysa backend 409 döner. */
export function deleteMediaAsset(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/media/${encodeURIComponent(id)}`, { method: 'DELETE' });
}
