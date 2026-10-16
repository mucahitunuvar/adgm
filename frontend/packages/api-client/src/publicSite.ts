import type { ApiClient } from './client';
import type { components, operations } from './schema';

type GetPublicSiteQuery = NonNullable<operations['GetPublicSite']['parameters']['query']>;
type GetPublicHomeQuery = NonNullable<operations['GetPublicHome']['parameters']['query']>;

export type PublicSiteResponse = components['schemas']['PublicSiteResponse'];
export type PublicHomeResponse = components['schemas']['PublicHomeResponse'];
export type PublicMenuItemResponse = components['schemas']['PublicMenuItemResponse'];

/** Faz 3 Koşu 1 (FRONTEND-ACIK-MADDELER.md §B madde 15'in kapanışı): backend artık bu uç için gerçek
 * yanıt şeması bildiriyor; dönüş tipi `unknown` yerine üretilen `PublicSiteResponse`'tur. */
export function getPublicSite(client: ApiClient, query?: GetPublicSiteQuery, revalidate = 60): Promise<PublicSiteResponse> {
  return client.request('/api/v1/public/site', { query, revalidate });
}

/** Aynı not GET /api/v1/public/home için de geçerlidir. */
export function getPublicHome(client: ApiClient, query?: GetPublicHomeQuery, revalidate = 60): Promise<PublicHomeResponse> {
  return client.request('/api/v1/public/home', { query, revalidate });
}
