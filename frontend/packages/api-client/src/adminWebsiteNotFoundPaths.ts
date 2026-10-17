import type { ApiClient } from './client';
import type { components } from './schema';

export type NotFoundLogResponse = components['schemas']['NotFoundLogResponse'];
export type PagedResultOfNotFoundLogResponse = components['schemas']['PagedResultOfNotFoundLogResponse'];
export type ConvertNotFoundPathToRedirectRequest = components['schemas']['ConvertNotFoundPathToRedirectRequest'];
export type ConvertNotFoundPathToRedirectResponse = components['schemas']['ConvertNotFoundPathToRedirectResponse'];

export interface ListNotFoundPathsParams {
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/not-found-paths`. Backend sık görülen yolu (hitCount desc) üstte döner. */
export function getNotFoundPaths(client: ApiClient, params: ListNotFoundPathsParams = {}): Promise<PagedResultOfNotFoundLogResponse> {
  const query: Record<string, number | undefined> = { ...params };
  return client.request('/api/v1/admin/website/not-found-paths', { query });
}

/** `DELETE /api/v1/admin/website/not-found-paths/{id}`. Kaydı yoksar/temizler. */
export function deleteNotFoundPath(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/not-found-paths/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

/** `POST /api/v1/admin/website/not-found-paths/{id}/convert-to-redirect`. Tek adımda yönlendirmeye dönüştürür. */
export function convertNotFoundPathToRedirect(
  client: ApiClient,
  id: string,
  body: ConvertNotFoundPathToRedirectRequest,
): Promise<ConvertNotFoundPathToRedirectResponse> {
  return client.request(`/api/v1/admin/website/not-found-paths/${encodeURIComponent(id)}/convert-to-redirect`, { method: 'POST', body });
}
