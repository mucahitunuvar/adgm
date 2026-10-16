import type { ApiClient } from './client';
import type { components, operations } from './schema';

export type PublicEventListItemResponse = components['schemas']['PublicEventListItemResponse'];
export type PagedResultOfPublicEventListItemResponse = components['schemas']['PagedResultOfPublicEventListItemResponse'];
export type PublicContentDetailEventResponse = components['schemas']['PublicContentDetailEventResponse'];
export type CreateEventRegistrationRequest = components['schemas']['CreateEventRegistrationRequest'];

export type ListPublicEventsParams = NonNullable<operations['GetPublicEvents']['parameters']['query']>;

/** ADR-024 §11.3: `registrationState`/`remainingSpots` backend tarafından hesaplanır, frontend hesaplamaz. */
export function listPublicEvents(
  client: ApiClient,
  params: ListPublicEventsParams = {},
  revalidate = 60,
): Promise<PagedResultOfPublicEventListItemResponse> {
  return client.request('/api/v1/public/events', { query: params, revalidate });
}

/** `.ics` yanıtı `text/calendar`dır; bu yalnız bağlantı (`<a href>`) içindir, fetch ile çağrılmaz. */
export function publicEventCalendarUrl(contentItemId: string, lang?: string): string {
  const query = lang ? `?lang=${encodeURIComponent(lang)}` : '';
  return `/api/v1/public/events/${encodeURIComponent(contentItemId)}/calendar.ics${query}`;
}

/** Kayıt durumu tarayıcıda canlı çekilir (K-07); bu çağrı her zaman `no-store`dur. */
export function createEventRegistration(
  client: ApiClient,
  contentItemId: string,
  request: CreateEventRegistrationRequest,
): Promise<void> {
  return client.request(`/api/v1/public/events/${encodeURIComponent(contentItemId)}/registrations`, {
    method: 'POST',
    body: request,
    cache: 'no-store',
  });
}

export function verifyEventRegistration(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/public/event-registrations/verifications', {
    method: 'POST',
    body: { token },
    cache: 'no-store',
  });
}

export function resendEventRegistrationVerification(
  client: ApiClient,
  request: { contentItemId: string; submissionToken: string | null; turnstileToken: string | null; website: string | null; email: string | null },
): Promise<void> {
  return client.request('/api/v1/public/event-registrations/verification-resends', {
    method: 'POST',
    body: request,
    cache: 'no-store',
  });
}

/** İptal idempotenttir (ADR-024 §11.2): zaten iptal edilmiş bir kayıt için tekrar çağrılırsa durum değişmeden döner. */
export function cancelEventRegistration(client: ApiClient, token: string): Promise<void> {
  return client.request('/api/v1/public/event-registrations/cancellations', {
    method: 'POST',
    body: { token },
    cache: 'no-store',
  });
}
