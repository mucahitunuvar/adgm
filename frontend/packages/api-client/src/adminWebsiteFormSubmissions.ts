import type { ApiClient, BlobResponse } from './client';
import type { components } from './schema';

export type FormSubmissionSummaryResponse = components['schemas']['FormSubmissionSummaryResponse'];
export type PagedResultOfFormSubmissionSummaryResponse = components['schemas']['PagedResultOfFormSubmissionSummaryResponse'];
export type FormSubmissionDetailResponse = components['schemas']['FormSubmissionDetailResponse'];
export type FormSubmissionAnswerResponse = components['schemas']['FormSubmissionAnswerResponse'];
export type FormSubmissionFileResponse = components['schemas']['FormSubmissionFileResponse'];
export type FormSubmissionAcceptedLegalVersionResponse = components['schemas']['FormSubmissionAcceptedLegalVersionResponse'];
export type FormSubmissionStatusHistoryEntryResponse = components['schemas']['FormSubmissionStatusHistoryEntryResponse'];
export type FormSubmissionInternalNoteResponse = components['schemas']['FormSubmissionInternalNoteResponse'];
export type ChangeFormSubmissionStatusRequest = components['schemas']['ChangeFormSubmissionStatusRequest'];
export type AssignFormSubmissionRequest = components['schemas']['AssignFormSubmissionRequest'];
export type AddFormSubmissionNoteRequest = components['schemas']['AddFormSubmissionNoteRequest'];

interface RowVersioned {
  rowVersion: string;
}

export interface ListFormSubmissionsParams {
  formKey?: string;
  status?: string;
  archived?: boolean;
  assignedToUserId?: string;
  from?: string;
  to?: string;
  referenceNumber?: string;
  page?: number;
  pageSize?: number;
}

/** `GET /api/v1/admin/website/form-submissions`. Kişisel veri (alan yanıtları) içerir - liste özet alanları PII taşımaz. */
export function getFormSubmissions(client: ApiClient, params: ListFormSubmissionsParams = {}): Promise<PagedResultOfFormSubmissionSummaryResponse> {
  const query: Record<string, string | number | boolean | undefined> = { ...params };
  return client.request('/api/v1/admin/website/form-submissions', { query });
}

/** `GET /api/v1/admin/website/form-submissions/{id}`. Kişisel veri - backend bu okumayı erişim kaydına yazar. */
export function getFormSubmissionById(client: ApiClient, id: string): Promise<FormSubmissionDetailResponse> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}`);
}

/**
 * `GET /api/v1/admin/website/form-submissions/{id}/files/{fileId}`. Dosya satır içi açılmaz - çağıran
 * `downloadBlob` ile indirmeyi tetikler (FRONTEND-FAZ2B-MASTER-PROMPT.md PII kuralları).
 */
export function downloadFormSubmissionFile(client: ApiClient, id: string, fileId: string): Promise<BlobResponse> {
  return client.requestBlob(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/files/${encodeURIComponent(fileId)}`);
}

export function changeFormSubmissionStatus(client: ApiClient, id: string, body: ChangeFormSubmissionStatusRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/status`, { method: 'POST', body });
}

/** `assignedToUserId: null` atamayı kaldırır. */
export function assignFormSubmission(client: ApiClient, id: string, body: AssignFormSubmissionRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/assign`, { method: 'POST', body });
}

export function addFormSubmissionNote(client: ApiClient, id: string, body: AddFormSubmissionNoteRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/notes`, { method: 'POST', body });
}

/** Yalnız kapanış durumundaki (Rejected/Completed) başvurular arşivlenebilir. */
export function archiveFormSubmission(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/archive`, { method: 'POST', body });
}

export function unarchiveFormSubmission(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/form-submissions/${encodeURIComponent(id)}/unarchive`, { method: 'POST', body });
}
