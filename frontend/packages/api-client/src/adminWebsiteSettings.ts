import type { ApiClient } from './client';
import type { components } from './schema';

export type SiteSettingsResponse = components['schemas']['SiteSettingsResponse'];
export type UpdateSiteSettingsIdentityRequest = components['schemas']['UpdateSiteSettingsIdentityRequest'];
export type UpdateSiteSettingsThemeRequest = components['schemas']['UpdateSiteSettingsThemeRequest'];
export type UpdateSiteSettingsContactRequest = components['schemas']['UpdateSiteSettingsContactRequest'];
export type UpdateSiteSettingsSocialLinkInput = components['schemas']['UpdateSiteSettingsSocialLinkInput'];
export type UpdateSiteSettingsBankAccountsRequest = components['schemas']['UpdateSiteSettingsBankAccountsRequest'];
export type UpdateSiteSettingsBankAccountInput = components['schemas']['UpdateSiteSettingsBankAccountInput'];
export type UpdateSiteSettingsFeaturesRequest = components['schemas']['UpdateSiteSettingsFeaturesRequest'];
export type UpdateSiteSettingsMaintenanceRequest = components['schemas']['UpdateSiteSettingsMaintenanceRequest'];
export type UpdateSiteSettingsMaintenanceTranslationInput = components['schemas']['UpdateSiteSettingsMaintenanceTranslationInput'];
export type UpdateSiteSettingsBotProtectionRequest = components['schemas']['UpdateSiteSettingsBotProtectionRequest'];
export type UpdateSiteSettingsSubmissionsRequest = components['schemas']['UpdateSiteSettingsSubmissionsRequest'];
export type UpdateSiteSettingsNewsletterRequest = components['schemas']['UpdateSiteSettingsNewsletterRequest'];
export type UpdateSiteSettingsCookieConsentRequest = components['schemas']['UpdateSiteSettingsCookieConsentRequest'];
export type UpdateSiteSettingsCookieConsentTranslationInput =
  components['schemas']['UpdateSiteSettingsCookieConsentTranslationInput'];
export type UpdateSiteSettingsEventsRequest = components['schemas']['UpdateSiteSettingsEventsRequest'];
export type UpdateSiteSettingsIdentityTranslationInput = components['schemas']['UpdateSiteSettingsIdentityTranslationInput'];

/** `GET /api/v1/admin/website/settings`. Yalnız Admin. */
export function getSiteSettings(client: ApiClient): Promise<SiteSettingsResponse> {
  return client.request('/api/v1/admin/website/settings');
}

/**
 * Her alt kaynak kendi `PUT` ucuna ve kendi `rowVersion`ına sahiptir (tek büyük form değil,
 * ADR-024 §12); eşzamanlı düzenleme 409 döner (`getApiErrorMessage` bunu ele alır).
 */
export function updateSiteSettingsIdentity(client: ApiClient, body: UpdateSiteSettingsIdentityRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/identity', { method: 'PUT', body });
}

export function updateSiteSettingsTheme(client: ApiClient, body: UpdateSiteSettingsThemeRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/theme', { method: 'PUT', body });
}

export function updateSiteSettingsContact(client: ApiClient, body: UpdateSiteSettingsContactRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/contact', { method: 'PUT', body });
}

export function updateSiteSettingsBankAccounts(client: ApiClient, body: UpdateSiteSettingsBankAccountsRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/bank-accounts', { method: 'PUT', body });
}

export function updateSiteSettingsFeatures(client: ApiClient, body: UpdateSiteSettingsFeaturesRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/features', { method: 'PUT', body });
}

export function updateSiteSettingsMaintenance(client: ApiClient, body: UpdateSiteSettingsMaintenanceRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/maintenance', { method: 'PUT', body });
}

export function updateSiteSettingsBotProtection(client: ApiClient, body: UpdateSiteSettingsBotProtectionRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/bot-protection', { method: 'PUT', body });
}

export function updateSiteSettingsSubmissions(client: ApiClient, body: UpdateSiteSettingsSubmissionsRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/submissions', { method: 'PUT', body });
}

export function updateSiteSettingsNewsletter(client: ApiClient, body: UpdateSiteSettingsNewsletterRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/newsletter', { method: 'PUT', body });
}

export function updateSiteSettingsCookieConsent(client: ApiClient, body: UpdateSiteSettingsCookieConsentRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/cookie-consent', { method: 'PUT', body });
}

export function updateSiteSettingsEvents(client: ApiClient, body: UpdateSiteSettingsEventsRequest): Promise<void> {
  return client.request('/api/v1/admin/website/settings/events', { method: 'PUT', body });
}
