import type { ApiClient } from './client';
import type { components } from './schema';

export type CookieConsentSummaryResponse = components['schemas']['CookieConsentSummaryResponse'];

export interface GetCookieConsentSummaryParams {
  /** UTC ISO 8601 (dönem başlangıcı), isteğe bağlı. */
  from?: string;
  /** UTC ISO 8601 (dönem sonu), isteğe bağlı. */
  to?: string;
}

/**
 * `GET /api/v1/admin/website/cookie-consents/summary`. Yalnız toplulaştırılmış sayılar/oranlar döner
 * (ADR-024 §12.3) - kayıtlar kimse bazında/IP/User-Agent taşımaz, bu yüzden kişisel veri değildir.
 */
export function getCookieConsentSummary(
  client: ApiClient,
  params: GetCookieConsentSummaryParams = {},
): Promise<CookieConsentSummaryResponse> {
  const query: Record<string, string | undefined> = { ...params };
  return client.request('/api/v1/admin/website/cookie-consents/summary', { query });
}
