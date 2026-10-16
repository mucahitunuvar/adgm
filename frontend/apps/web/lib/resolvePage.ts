import { cache } from 'react';
import { resolveRoute, type RouteResolutionResponse } from '@gm/api-client';
import { createServerApiClient } from './apiClient';

/**
 * `generateMetadata` ve sayfa bileşeni aynı yolu ayrı ayrı çözerse React bu isteği tek render
 * geçişinde otomatik hafızaya alır (fetch memoization); `cache()` bunu açıkça belgeler ve test
 * ortamında da (fetch memoization olmadan) tek çağrıya indirir.
 */
export const resolvePage = cache(async (path: string): Promise<RouteResolutionResponse> => {
  const client = createServerApiClient();
  return resolveRoute(client, { path });
});

export function pathFromSlug(slug?: string[]): string {
  return slug && slug.length > 0 ? `/${slug.join('/')}` : '/';
}
