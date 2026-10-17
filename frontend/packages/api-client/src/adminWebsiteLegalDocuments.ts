import type { ApiClient } from './client';
import type { components } from './schema';

export type LegalDocumentSummaryResponse = components['schemas']['LegalDocumentSummaryResponse'];
export type LegalDocumentDetailResponse = components['schemas']['LegalDocumentDetailResponse'];
export type LegalDocumentTranslationResponse = components['schemas']['LegalDocumentTranslationResponse'];
export type LegalDocumentVersionResponse = components['schemas']['LegalDocumentVersionResponse'];
export type LegalDocumentVersionTranslationResponse = components['schemas']['LegalDocumentVersionTranslationResponse'];
export type CreateLegalDocumentRequest = components['schemas']['CreateLegalDocumentRequest'];
export type CreateLegalDocumentResponse = components['schemas']['CreateLegalDocumentResponse'];
export type UpdateLegalDocumentTranslationRequest = components['schemas']['UpdateLegalDocumentTranslationRequest'];
export type CreateLegalDocumentDraftRequest = components['schemas']['CreateLegalDocumentDraftRequest'];
export type CreateLegalDocumentDraftResponse = components['schemas']['CreateLegalDocumentDraftResponse'];
export type UpdateLegalDocumentDraftBodyRequest = components['schemas']['UpdateLegalDocumentDraftBodyRequest'];
export type PublishLegalDocumentDraftRequest = components['schemas']['PublishLegalDocumentDraftRequest'];
export type DeleteLegalDocumentDraftRequest = components['schemas']['DeleteLegalDocumentDraftRequest'];

/** `GET /api/v1/admin/website/legal-documents`. Sayfalama yok (yönetilebilir sayıda belge beklenir). */
export function getLegalDocuments(client: ApiClient): Promise<LegalDocumentSummaryResponse[]> {
  return client.request('/api/v1/admin/website/legal-documents');
}

/** `GET /api/v1/admin/website/legal-documents/{id}`. Belge başlıkları + tüm sürümlerin (taslak dahil) gövdeleri. */
export function getLegalDocumentById(client: ApiClient, id: string): Promise<LegalDocumentDetailResponse> {
  return client.request(`/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}`);
}

/** `POST /api/v1/admin/website/legal-documents`. `key` sonradan değiştirilemez. */
export function createLegalDocument(client: ApiClient, body: CreateLegalDocumentRequest): Promise<CreateLegalDocumentResponse> {
  return client.request('/api/v1/admin/website/legal-documents', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/legal-documents/{id}/translations/{languageCode}`. Belge başlığı (sürümden bağımsız). */
export function updateLegalDocumentTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateLegalDocumentTranslationRequest,
): Promise<void> {
  return client.request(
    `/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`,
    { method: 'PUT', body },
  );
}

/** `DELETE /api/v1/admin/website/legal-documents/{id}`. Backend hiç yayınlanmış sürümü yoksa izin verir. */
export function deleteLegalDocument(client: ApiClient, id: string): Promise<void> {
  return client.request(`/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

/** `POST /api/v1/admin/website/legal-documents/{id}/versions/draft`. Aynı anda tek taslak olabilir. */
export function createLegalDocumentDraft(
  client: ApiClient,
  id: string,
  body: CreateLegalDocumentDraftRequest,
): Promise<CreateLegalDocumentDraftResponse> {
  return client.request(`/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}/versions/draft`, { method: 'POST', body });
}

/** `DELETE /api/v1/admin/website/legal-documents/{id}/versions/draft`. Taslağı atar. */
export function deleteLegalDocumentDraft(client: ApiClient, id: string, body: DeleteLegalDocumentDraftRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}/versions/draft`, { method: 'DELETE', body });
}

/** `PUT /api/v1/admin/website/legal-documents/{id}/versions/draft/translations/{languageCode}`. Gövde dile göre. */
export function updateLegalDocumentDraftBody(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateLegalDocumentDraftBodyRequest,
): Promise<void> {
  return client.request(
    `/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}/versions/draft/translations/${encodeURIComponent(languageCode)}`,
    { method: 'PUT', body },
  );
}

/** `POST /api/v1/admin/website/legal-documents/{id}/versions/draft/publish`. Yayınlanan sürüm bir daha değiştirilemez. */
export function publishLegalDocumentDraft(client: ApiClient, id: string, body: PublishLegalDocumentDraftRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/legal-documents/${encodeURIComponent(id)}/versions/draft/publish`, { method: 'POST', body });
}
