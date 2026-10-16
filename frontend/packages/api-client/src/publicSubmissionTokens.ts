import type { ApiClient } from './client';
import type { components } from './schema';

export type GetSubmissionTokenResponse = components['schemas']['GetSubmissionTokenResponse'];

/**
 * Anonim form gönderimlerinin ilk adımı (ADR-024 §12.3): gönderim jetonu → honeypot → minimum
 * doldurma süresi → Turnstile → gönderim. Jeton kısa ömürlüdür, loglanmaz.
 */
export function createSubmissionToken(client: ApiClient): Promise<GetSubmissionTokenResponse> {
  return client.request('/api/v1/public/submission-tokens', { cache: 'no-store' });
}
