import type { ApiClient, BlobResponse } from './client';
import type { components } from './schema';

export type NewsletterSubscriberSummaryResponse = components['schemas']['NewsletterSubscriberSummaryResponse'];
export type PagedResultOfNewsletterSubscriberSummaryResponse = components['schemas']['PagedResultOfNewsletterSubscriberSummaryResponse'];

export interface ListNewsletterSubscribersParams {
  status?: string;
  language?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/newsletter-subscribers`. Kişisel veri (e-posta) içerir - backend kendi erişim kaydını yazar. */
export function getNewsletterSubscribers(
  client: ApiClient,
  params: ListNewsletterSubscribersParams = {},
): Promise<PagedResultOfNewsletterSubscriberSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/newsletter-subscribers', { query });
}

/** `GET /api/v1/admin/website/newsletter-subscribers/export` (CSV). Backend formül enjeksiyonuna karşı hücreleri kaçışlar. */
export function exportNewsletterSubscribers(
  client: ApiClient,
  params: Pick<ListNewsletterSubscribersParams, 'status' | 'language'> = {},
): Promise<BlobResponse> {
  const query: Record<string, string | undefined> = { ...params };
  return client.requestBlob('/api/v1/admin/website/newsletter-subscribers/export', { query });
}

/** `DELETE /api/v1/admin/website/newsletter-subscribers/{id}`. KVKK silme talebi; geri alınamaz. */
export function deleteNewsletterSubscriber(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/newsletter-subscribers/${encodeURIComponent(id)}`, { method: 'DELETE' });
}
