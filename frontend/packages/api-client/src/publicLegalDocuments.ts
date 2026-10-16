import type { ApiClient } from './client';
import type { components, operations } from './schema';

export type PublicLegalDocumentResponse = components['schemas']['PublicLegalDocumentResponse'];
export type PublicLegalDocumentVersionResponse = components['schemas']['PublicLegalDocumentVersionResponse'];

type GetPublicLegalDocumentQuery = NonNullable<operations['GetPublicLegalDocument']['parameters']['query']>;
type GetPublicLegalDocumentVersionQuery = NonNullable<operations['GetPublicLegalDocumentVersion']['parameters']['query']>;

/** Yayınlanan sürüm değiştirilemez (ADR-024 §12.1); bu her zaman yürürlükteki sürümü döner. */
export function getPublicLegalDocument(
  client: ApiClient,
  key: string,
  query?: GetPublicLegalDocumentQuery,
  revalidate = 60,
): Promise<PublicLegalDocumentResponse> {
  return client.request(`/api/v1/public/legal-documents/${encodeURIComponent(key)}`, { query, revalidate });
}

export function getPublicLegalDocumentVersion(
  client: ApiClient,
  key: string,
  versionNumber: number,
  query?: GetPublicLegalDocumentVersionQuery,
  revalidate = 60,
): Promise<PublicLegalDocumentVersionResponse> {
  return client.request(`/api/v1/public/legal-documents/${encodeURIComponent(key)}/versions/${versionNumber}`, { query, revalidate });
}
