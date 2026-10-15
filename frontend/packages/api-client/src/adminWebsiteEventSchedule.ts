import type { ApiClient } from './client';
import type { components } from './schema';

export type EventScheduleDetailResponse = components['schemas']['EventScheduleDetailResponse'];
export type EventScheduleTranslationResponse = components['schemas']['EventScheduleTranslationResponse'];
export type UpsertEventScheduleRequest = components['schemas']['UpsertEventScheduleRequest'];
export type CancelEventScheduleRequest = components['schemas']['CancelEventScheduleRequest'];
export type ReactivateEventScheduleRequest = components['schemas']['ReactivateEventScheduleRequest'];

function basePath(contentItemId: string): string {
  return `/api/v1/admin/website/contents/${encodeURIComponent(contentItemId)}/event`;
}

/** İçeriğin `SupportsEvent` etkinlik tanımı yoksa 404 döner - "henüz tanımlanmamış" sayılır. */
export function getEventScheduleByContentItemId(client: ApiClient, contentItemId: string): Promise<EventScheduleDetailResponse> {
  return client.request(basePath(contentItemId));
}

/** `RowVersion` boşsa oluşturur, doluysa günceller (aynı uç, optimistic concurrency). */
export function upsertEventSchedule(
  client: ApiClient,
  contentItemId: string,
  body: UpsertEventScheduleRequest,
): Promise<EventScheduleDetailResponse> {
  return client.request(basePath(contentItemId), { method: 'PUT', body });
}

export function cancelEventSchedule(client: ApiClient, contentItemId: string, body: CancelEventScheduleRequest): Promise<void> {
  return client.request(`${basePath(contentItemId)}/cancel`, { method: 'POST', body });
}

export function reactivateEventSchedule(client: ApiClient, contentItemId: string, body: ReactivateEventScheduleRequest): Promise<void> {
  return client.request(`${basePath(contentItemId)}/reactivate`, { method: 'POST', body });
}

export function deleteEventSchedule(client: ApiClient, contentItemId: string): Promise<void> {
  return client.request(basePath(contentItemId), { method: 'DELETE' });
}
