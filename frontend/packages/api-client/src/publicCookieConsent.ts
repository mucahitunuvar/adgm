import type { ApiClient } from './client';
import type { components } from './schema';
import { CookieConsentAction } from './enums.generated';

export type CreateCookieConsentRecordRequest = components['schemas']['CreateCookieConsentRecordRequest'];

/**
 * ADR-024 §13: kimliksiz, anonim bir çerez tercihi kaydı. `consentId` sunucuda hiçbir kullanıcıyla
 * eşlenmez - yalnız tarayıcının kendi `consentStore` çerezinde tuttuğu rastgele bir değerdir.
 */
export function createCookieConsentRecord(
  client: ApiClient,
  request: { consentId: string; categories: readonly string[]; policyVersion: number; action: CookieConsentAction },
): Promise<void> {
  return client.request('/api/v1/public/cookie-consents', {
    method: 'POST',
    body: {
      consentId: request.consentId,
      categories: [...request.categories],
      policyVersion: request.policyVersion,
      action: request.action,
    } satisfies CreateCookieConsentRecordRequest,
  });
}
