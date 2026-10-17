import type { ApiClient } from './client';
import type { components } from './schema';

export type RedirectResponse = components['schemas']['RedirectResponse'];
export type PagedResultOfRedirectResponse = components['schemas']['PagedResultOfRedirectResponse'];
export type CreateRedirectRequest = components['schemas']['CreateRedirectRequest'];
export type CreateRedirectResponse = components['schemas']['CreateRedirectResponse'];
export type UpdateRedirectRequest = components['schemas']['UpdateRedirectRequest'];

export interface ListRedirectsParams {
  languageCode?: string;
  isAutomatic?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/redirects`. */
export function getRedirects(client: ApiClient, params: ListRedirectsParams = {}): Promise<PagedResultOfRedirectResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/redirects', { query });
}

/** `POST /api/v1/admin/website/redirects`. Kaynak yol dil içinde benzersizdir; zincir backend'de reddedilir. */
export function createRedirect(client: ApiClient, body: CreateRedirectRequest): Promise<CreateRedirectResponse> {
  return client.request('/api/v1/admin/website/redirects', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/redirects/{id}`. Yalnız elle oluşturulanlar düzenlenebilir (`isAutomatic: false`). */
export function updateRedirect(client: ApiClient, id: string, body: UpdateRedirectRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/redirects/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

/** `DELETE /api/v1/admin/website/redirects/{id}`. Otomatik oluşturulanlar da silinebilir. */
export function deleteRedirect(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/redirects/${encodeURIComponent(id)}`, { method: 'DELETE' });
}
