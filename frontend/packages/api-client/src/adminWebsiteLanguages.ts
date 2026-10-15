import type { ApiClient } from './client';
import type { components } from './schema';

export type SiteLanguageResponse = components['schemas']['SiteLanguageResponse'];
export type GetSiteLanguagesResponse = components['schemas']['GetSiteLanguagesResponse'];
export type CreateSiteLanguageRequest = components['schemas']['CreateSiteLanguageRequest'];
export type CreateSiteLanguageResponse = components['schemas']['CreateSiteLanguageResponse'];
export type UpdateSiteLanguageRequest = components['schemas']['UpdateSiteLanguageRequest'];

/** `GET /api/v1/admin/website/languages`. Yalnız Admin. */
export function getSiteLanguages(client: ApiClient): Promise<GetSiteLanguagesResponse> {
  return client.request('/api/v1/admin/website/languages');
}

/** `POST /api/v1/admin/website/languages`. */
export function createSiteLanguage(client: ApiClient, body: CreateSiteLanguageRequest): Promise<CreateSiteLanguageResponse> {
  return client.request('/api/v1/admin/website/languages', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/languages/{id}`. Dil kodu değiştirilemez (backend ucu kabul etmiyor). */
export function updateSiteLanguage(client: ApiClient, id: string, body: UpdateSiteLanguageRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/languages/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

export function activateSiteLanguage(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/languages/${encodeURIComponent(id)}/activate`, { method: 'POST' });
}

export function deactivateSiteLanguage(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/languages/${encodeURIComponent(id)}/deactivate`, { method: 'POST' });
}

/** Varsayılan dil pasif yapılamaz/silinemez (backend invariant); arayüz bunu `isDefault` ile yansıtır. */
export function setDefaultSiteLanguage(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/languages/${encodeURIComponent(id)}/set-default`, { method: 'POST' });
}
