import type { ApiClient } from './client';
import type { components, operations } from './schema';

type ResolveRouteQuery = NonNullable<operations['ResolveRoute']['parameters']['query']>;

export type RouteResolutionResponse = components['schemas']['RouteResolutionResponse'];

/**
 * ADR-024 §15: `path`'i Home/Detail/Listing/Redirect/NotFound'a çözer. `no-store` ile çağrılmalıdır
 * (ortak ilke: arama/ilan/etkinlik/rota çözümleme önbelleğe girmez) - revalidate parametresi yok.
 */
export function resolveRoute(client: ApiClient, query: ResolveRouteQuery): Promise<RouteResolutionResponse> {
  return client.request('/api/v1/public/routes/resolve', { query, cache: 'no-store' });
}
