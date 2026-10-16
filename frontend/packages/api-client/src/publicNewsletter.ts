import type { ApiClient } from './client';
import type { components } from './schema';

export type SubscribeToNewsletterRequest = components['schemas']['SubscribeToNewsletterRequest'];

/** Çift onaylıdır (ADR-024 §14): abonelik, e-postadaki linke tıklanınca aktifleşir. */
export function subscribeToNewsletter(client: ApiClient, request: SubscribeToNewsletterRequest): Promise<void> {
  return client.request('/api/v1/public/newsletter/subscriptions', { method: 'POST', body: request, cache: 'no-store' });
}

export function confirmNewsletterSubscription(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/public/newsletter/confirmations', { method: 'POST', body: { token }, cache: 'no-store' });
}

export function unsubscribeFromNewsletter(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/public/newsletter/unsubscriptions', { method: 'POST', body: { token }, cache: 'no-store' });
}
