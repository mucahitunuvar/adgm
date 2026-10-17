import type { ApiClient } from './client';
import type { components } from './schema';

export type FormDefinitionSummaryResponse = components['schemas']['FormDefinitionSummaryResponse'];
export type FormDefinitionDetailResponse = components['schemas']['FormDefinitionDetailResponse'];
export type FormDefinitionTranslationResponse = components['schemas']['FormDefinitionTranslationResponse'];
export type FormExplicitConsentResponse = components['schemas']['FormExplicitConsentResponse'];
export type FormFieldResponse = components['schemas']['FormFieldResponse'];
export type FormFieldOptionResponse = components['schemas']['FormFieldOptionResponse'];
export type FormFieldTranslationResponse = components['schemas']['FormFieldTranslationResponse'];
export type FormFieldOptionTranslationResponse = components['schemas']['FormFieldOptionTranslationResponse'];
export type FormFieldInput = components['schemas']['FormFieldInput'];
export type FormFieldOptionInput = components['schemas']['FormFieldOptionInput'];
export type FormFieldTranslationInput = components['schemas']['FormFieldTranslationInput'];
export type FormFieldOptionTranslationInput = components['schemas']['FormFieldOptionTranslationInput'];
export type CreateFormDefinitionRequest = components['schemas']['CreateFormDefinitionRequest'];
export type CreateFormDefinitionResponse = components['schemas']['CreateFormDefinitionResponse'];
export type CreateFormDefinitionExplicitConsentInput = components['schemas']['CreateFormDefinitionExplicitConsentInput'];
export type UpdateFormDefinitionRequest = components['schemas']['UpdateFormDefinitionRequest'];
export type UpdateFormDefinitionExplicitConsentInput = components['schemas']['UpdateFormDefinitionExplicitConsentInput'];
export type UpdateFormDefinitionTranslationRequest = components['schemas']['UpdateFormDefinitionTranslationRequest'];
export type SetFormFieldsRequest = components['schemas']['SetFormFieldsRequest'];

interface RowVersioned {
  rowVersion: string;
}

/** `GET /api/v1/admin/website/forms`. Sayfalama yok (yönetilebilir sayıda form beklenir). */
export function getFormDefinitions(client: ApiClient): Promise<FormDefinitionSummaryResponse[]> {
  return client.request('/api/v1/admin/website/forms');
}

/** `GET /api/v1/admin/website/forms/{id}`. */
export function getFormDefinitionById(client: ApiClient, id: string): Promise<FormDefinitionDetailResponse> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}`);
}

/** `POST /api/v1/admin/website/forms`. Varsayılan dilin çevirisini de birlikte oluşturur, alansız. */
export function createFormDefinition(client: ApiClient, body: CreateFormDefinitionRequest): Promise<CreateFormDefinitionResponse> {
  return client.request('/api/v1/admin/website/forms', { method: 'POST', body });
}

/** `PUT /api/v1/admin/website/forms/{id}`. `key` değiştirilemez. */
export function updateFormDefinition(client: ApiClient, id: string, body: UpdateFormDefinitionRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}`, { method: 'PUT', body });
}

/** Başvurusu varsa backend 409 ile reddeder (form tanımı silinemez). */
export function deleteFormDefinition(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}`, { method: 'DELETE', body });
}

export function updateFormDefinitionTranslation(
  client: ApiClient,
  id: string,
  languageCode: string,
  body: UpdateFormDefinitionTranslationRequest,
): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'PUT',
    body,
  });
}

/** Varsayılan dilin çevirisi silinemez (backend invariant); arayüz bu seçeneği göstermez. */
export function deleteFormDefinitionTranslation(client: ApiClient, id: string, languageCode: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}/translations/${encodeURIComponent(languageCode)}`, {
    method: 'DELETE',
    body,
  });
}

/** Tüm alan listesini değiştirir (sıra dahil) - tekil alan ucu yok, liste bütün olarak gönderilir. */
export function setFormFields(client: ApiClient, id: string, body: SetFormFieldsRequest): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}/fields`, { method: 'PUT', body });
}

export function activateFormDefinition(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}/activate`, { method: 'POST', body });
}

export function deactivateFormDefinition(client: ApiClient, id: string, body: RowVersioned): Promise<void> {
  return client.request(`/api/v1/admin/website/forms/${encodeURIComponent(id)}/deactivate`, { method: 'POST', body });
}
