import type { ApiClient } from './client';
import type { components } from './schema';

export type ImpactMetricSummaryResponse = components['schemas']['ImpactMetricSummaryResponse'];
export type PagedResultOfImpactMetricSummaryResponse = components['schemas']['PagedResultOfImpactMetricSummaryResponse'];
export type ImpactMetricDetailResponse = components['schemas']['ImpactMetricDetailResponse'];
export type ImpactMetricTranslationResponse = components['schemas']['ImpactMetricTranslationResponse'];
export type CreateImpactMetricRequest = components['schemas']['CreateImpactMetricRequest'];
export type CreateImpactMetricResponse = components['schemas']['CreateImpactMetricResponse'];
export type UpdateImpactMetricRequest = components['schemas']['UpdateImpactMetricRequest'];
export type UpdateImpactMetricTranslationRequest = components['schemas']['UpdateImpactMetricTranslationRequest'];

export interface ListImpactMetricsParams {
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

export function getImpactMetrics(
  client: ApiClient,
  params: ListImpactMetricsParams = {},
): Promise<PagedResultOfImpactMetricSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/impact-metrics', { query });
}

export function getImpactMetricById(client: ApiClient, id: string): Promise<ImpactMetricDetailResponse> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}`);
}

export function createImpactMetric(client: ApiClient, body: CreateImpactMetricRequest): Promise<CreateImpactMetricResponse> {
  return client.request('/api/v1/admin/website/impact-metrics', { method: 'POST', body });
}

/** Değer/ikon/sıra - çeviriler (etiket/birim/dönem/kaynak) ayrı uçtan (`updateImpactMetricTranslation`). */
export function updateImpactMetric(client: ApiClient, id: string, body: UpdateImpactMetricRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

export function deleteImpactMetric(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function activateImpactMetric(client: ApiClient, id: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}/activate`, {
    method: 'POST',
    body: { rowVersion },
  });
}

export function deactivateImpactMetric(client: ApiClient, id: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}/deactivate`, {
    method: 'POST',
    body: { rowVersion },
  });
}

export function updateImpactMetricTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateImpactMetricTranslationRequest,
): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'PUT',
    body,
  });
}

/** Varsayılan dilin çevirisi silinemez (backend invariant); arayüz bu seçeneği göstermez. */
export function deleteImpactMetricTranslation(client: ApiClient, id: string, languageCode: string, rowVersion: string): Promise<void> {
  return client.request(`/api/v1/admin/website/impact-metrics/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'DELETE',
    body: { rowVersion },
  });
}
