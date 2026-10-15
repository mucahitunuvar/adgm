import type { ApiClient } from './client';
import type { components } from './schema';

export type VideoSummaryResponse = components['schemas']['VideoSummaryResponse'];
export type PagedResultOfVideoSummaryResponse = components['schemas']['PagedResultOfVideoSummaryResponse'];

export interface ListVideosParams {
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/videos`. İçerik videoları (`SetContentItemVideos`) bu kütüphaneden seçilir. */
export function getVideos(client: ApiClient, params: ListVideosParams = {}): Promise<PagedResultOfVideoSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/videos', { query });
}
