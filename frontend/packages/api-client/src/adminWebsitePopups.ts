import type { ApiClient } from './client';
import type { components } from './schema';

export type PopupSummaryResponse = components['schemas']['PopupSummaryResponse'];
export type PagedResultOfPopupSummaryResponse = components['schemas']['PagedResultOfPopupSummaryResponse'];
export type PopupDetailResponse = components['schemas']['PopupDetailResponse'];
export type PopupImageResponse = components['schemas']['PopupImageResponse'];
export type PopupLinkResponse = components['schemas']['PopupLinkResponse'];
export type PopupTargetingResponse = components['schemas']['PopupTargetingResponse'];
export type PopupTranslationResponse = components['schemas']['PopupTranslationResponse'];
export type LinkTargetDto = components['schemas']['LinkTargetDto'];
export type PopupTargetingInput = components['schemas']['PopupTargetingInput'];
export type CreatePopupRequest = components['schemas']['CreatePopupRequest'];
export type CreatePopupResponse = components['schemas']['CreatePopupResponse'];
export type UpdatePopupRequest = components['schemas']['UpdatePopupRequest'];
export type UpdatePopupTranslationRequest = components['schemas']['UpdatePopupTranslationRequest'];

export interface ListPopupsParams {
  displayMode?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export function getPopups(client: ApiClient, params: ListPopupsParams = {}): Promise<PagedResultOfPopupSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/popups', { query });
}

export function getPopupById(client: ApiClient, id: string): Promise<PopupDetailResponse> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}`);
}

export function createPopup(client: ApiClient, body: CreatePopupRequest): Promise<CreatePopupResponse> {
  return client.request('/api/v1/admin/website/popups', { method: 'POST', body });
}

export function updatePopup(client: ApiClient, id: string, body: UpdatePopupRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

export function deletePopup(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function activatePopup(client: ApiClient, id: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}/activate`, { method: 'POST', body: { rowVersion } });
}

export function deactivatePopup(client: ApiClient, id: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}/deactivate`, { method: 'POST', body: { rowVersion } });
}

export function updatePopupTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdatePopupTranslationRequest,
): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'PUT',
    body,
  });
}

export function deletePopupTranslation(client: ApiClient, id: string, languageCode: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/popups/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'DELETE',
    body: { rowVersion },
  });
}
