import type { ApiClient } from './client';
import type { components, operations } from './schema';

export type PublicContentListResponse = components['schemas']['PublicContentListResponse'];
export type PublicContentListItemResponse = components['schemas']['PublicContentListItemResponse'];
export type PublicContentDetailResponse = components['schemas']['PublicContentDetailResponse'];
export type PublicContentCategoryTreeItemResponse = components['schemas']['PublicContentCategoryTreeItemResponse'];

export type ListPublicContentsParams = NonNullable<operations['GetPublicContents']['parameters']['query']>;
type GetPublicContentByIdQuery = NonNullable<operations['GetPublicContentById']['parameters']['query']>;

/** ADR-024 §15: `type` = ContentType.Key, `category`/`tag` = slug (dil içinde). */
export function listPublicContents(
  client: ApiClient,
  params: ListPublicContentsParams,
  revalidate = 60,
): Promise<PublicContentListResponse> {
  const isSearch = typeof params.search === 'string' && params.search.length > 0;
  return client.request('/api/v1/public/contents', {
    query: params,
    // ADR-024 §17: arama sorguları sınırsız anahtar alanı nedeniyle sunucuda cache'lenmez.
    ...(isSearch ? { cache: 'no-store' as const } : { revalidate }),
  });
}

export function getPublicContentById(
  client: ApiClient,
  id: string,
  query?: GetPublicContentByIdQuery,
  revalidate = 60,
): Promise<PublicContentDetailResponse> {
  return client.request(`/api/v1/public/contents/${encodeURIComponent(id)}`, { query, revalidate });
}
