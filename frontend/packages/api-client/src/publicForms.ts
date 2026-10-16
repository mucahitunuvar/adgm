import type { ApiClient } from './client';
import type { components, operations } from './schema';

export type PublicFormDefinitionResponse = components['schemas']['PublicFormDefinitionResponse'];
export type PublicFormFieldResponse = components['schemas']['PublicFormFieldResponse'];
export type SubmitFormSubmissionResponse = components['schemas']['SubmitFormSubmissionResponse'];

type GetPublicFormQuery = NonNullable<operations['GetPublicForm']['parameters']['query']>;

export function getPublicForm(client: ApiClient, key: string, query?: GetPublicFormQuery): Promise<PublicFormDefinitionResponse> {
  return client.request(`/api/v1/public/forms/${encodeURIComponent(key)}`, { query, cache: 'no-store' });
}

/**
 * Alan seti dinamiktir (ADR-024 §12.2, `SubmitFormSubmissionEndpoint.cs`): backend ham
 * `IFormCollection` okur, `token/turnstileToken/website/lang/acceptedPrivacyNoticeVersion/
 * explicitConsents/contentItemId` dışındaki her alanı form cevabı olarak kabul eder. Bu yüzden
 * burada `FormData` doğrudan gönderilir, JSON'a çevrilmez (dosya alanları da aynı gövdede taşınır).
 */
export function submitFormSubmission(client: ApiClient, key: string, formData: FormData): Promise<SubmitFormSubmissionResponse> {
  return client.request(`/api/v1/public/forms/${encodeURIComponent(key)}/submissions`, {
    method: 'POST',
    body: formData,
    cache: 'no-store',
  });
}
