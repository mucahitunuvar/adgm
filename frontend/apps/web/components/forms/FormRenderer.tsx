'use client';

import { useState, type FormEvent } from 'react';
import {
  createApiClient,
  createSubmissionToken,
  submitFormSubmission,
  ApiError,
  getApiErrorMessage,
  FormFieldType,
} from '@gm/api-client';
import type { PublicFormDefinitionResponse, SubmitFormSubmissionResponse } from '@gm/api-client';
import { TextField, TextAreaField, SelectField, CheckboxField, safeUrl } from '@gm/ui';
import { HoneypotField } from './HoneypotField';
import { TurnstileWidget } from './TurnstileWidget';

const KNOWN_FIELD_TYPES = new Set<string>(Object.values(FormFieldType));

interface FormRendererProps {
  formKey: string;
  definition: PublicFormDefinitionResponse;
  contentItemId?: string;
  turnstileSiteKey: string;
}

/**
 * ADR-024 §12.2: alan seti tamamen dinamiktir (`definition.fields`); tanınmayan bir alan tipi
 * sayfayı çökertmez, "desteklenmeyen alan" olarak işaretlenir ve gönderim engellenir (bilinçli
 * güvenli atlama - aynı blok registry ilkesi, master prompt §3D).
 */
export function FormRenderer({ formKey, definition, contentItemId, turnstileSiteKey }: FormRendererProps) {
  const [phase, setPhase] = useState<'idle' | 'submitting' | 'success' | 'error'>('idle');
  const [success, setSuccess] = useState<SubmitFormSubmissionResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);

  const hasUnsupportedField = definition.fields.some((field) => !KNOWN_FIELD_TYPES.has(field.type));

  async function handleSubmit(formEvent: FormEvent<HTMLFormElement>) {
    formEvent.preventDefault();
    setError(null);
    setFieldErrors({});

    const formData = new FormData(formEvent.currentTarget);
    const explicitConsents = definition.explicitConsents.map((consent) => {
      const checked = formData.get(`consent:${consent.key}`) === 'on';
      formData.delete(`consent:${consent.key}`);
      if (consent.isRequired && !checked) {
        throw new Error(`ExplicitConsent:${consent.key}`);
      }
      return { key: consent.key, version: Number(consent.versionNumber), accepted: checked };
    });

    setPhase('submitting');
    try {
      const client = createApiClient({ baseUrl: window.location.origin });
      const { token } = await createSubmissionToken(client);
      formData.set('token', token);
      formData.set('turnstileToken', turnstileToken ?? '');
      formData.set('lang', 'tr');
      formData.set('acceptedPrivacyNoticeVersion', String(definition.privacyNotice.versionNumber));
      formData.set('explicitConsents', JSON.stringify(explicitConsents));
      if (contentItemId) formData.set('contentItemId', contentItemId);

      const response = await submitFormSubmission(client, formKey, formData);
      setSuccess(response);
      setPhase('success');
    } catch (err) {
      if (err instanceof Error && err.message.startsWith('ExplicitConsent:')) {
        setError('Lütfen zorunlu onay kutularını işaretleyin.');
        setPhase('idle');
        return;
      }
      if (err instanceof ApiError && err.status === 400 && err.errors) {
        const mapped: Record<string, string> = {};
        for (const [key, messages] of Object.entries(err.errors)) {
          mapped[key] = messages[0] ?? '';
        }
        setFieldErrors(mapped);
      }
      setError(getApiErrorMessage(err));
      setPhase('idle');
    }
  }

  if (phase === 'success' && success) {
    return (
      <div className="form-success" role="status">
        <p>{success.successMessage}</p>
        <p className="form-success__reference">Referans numarası: {success.referenceNumber}</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="form-renderer" noValidate>
      <HoneypotField />
      {definition.fields.map((field) => (
        <DynamicField key={field.key} field={field} error={fieldErrors[field.key]} />
      ))}

      {hasUnsupportedField && <p className="form-renderer__warning">Bu formda desteklenmeyen bir alan var; gönderim şu an kapalı.</p>}

      <p className="form-renderer__privacy">
        Bu formu göndererek{' '}
        <a href={safeUrl(`/legal/${definition.privacyNotice.key}`)} target="_blank" rel="noopener noreferrer">
          {definition.privacyNotice.title}
        </a>
        &apos;ni okuduğunuzu kabul edersiniz.
      </p>
      {definition.explicitConsents.map((consent) => (
        <CheckboxField
          key={consent.key}
          id={`consent-${consent.key}`}
          name={`consent:${consent.key}`}
          label={
            consent.isRequired
              ? `${consent.title} (zorunlu)`
              : consent.title
          }
        />
      ))}

      {turnstileSiteKey && <TurnstileWidget siteKey={turnstileSiteKey} onToken={setTurnstileToken} />}
      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}
      <button type="submit" className="button button--primary" disabled={phase === 'submitting' || hasUnsupportedField}>
        {phase === 'submitting' ? 'Gönderiliyor…' : definition.submitButtonLabel}
      </button>
    </form>
  );
}

function DynamicField({ field, error }: { field: PublicFormDefinitionResponse['fields'][number]; error?: string }) {
  const id = `form-field-${field.key}`;
  const hint = field.helpText ?? undefined;

  switch (field.type) {
    case FormFieldType.Text:
    case FormFieldType.Email:
    case FormFieldType.Phone:
      return (
        <TextField
          id={id}
          name={field.key}
          label={field.label}
          type={field.type === FormFieldType.Email ? 'email' : field.type === FormFieldType.Phone ? 'tel' : 'text'}
          placeholder={field.placeholder ?? undefined}
          hint={hint}
          error={error}
          required={field.isRequired}
          minLength={field.minLength != null ? Number(field.minLength) : undefined}
          maxLength={field.maxLength != null ? Number(field.maxLength) : undefined}
        />
      );
    case FormFieldType.Textarea:
      return (
        <TextAreaField
          id={id}
          name={field.key}
          label={field.label}
          placeholder={field.placeholder ?? undefined}
          hint={hint}
          error={error}
          required={field.isRequired}
          minLength={field.minLength != null ? Number(field.minLength) : undefined}
          maxLength={field.maxLength != null ? Number(field.maxLength) : undefined}
        />
      );
    case FormFieldType.Select:
      return (
        <SelectField id={id} name={field.key} label={field.label} hint={hint} error={error} required={field.isRequired} defaultValue="">
          <option value="" disabled>
            Seçiniz
          </option>
          {field.options.map((option) => (
            <option key={option.key} value={option.key}>
              {option.label}
            </option>
          ))}
        </SelectField>
      );
    case FormFieldType.MultiSelect:
      return (
        <fieldset className="form-field">
          <legend>
            {field.label}
            {field.isRequired ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
          </legend>
          {hint && <p className="field-hint">{hint}</p>}
          {field.options.map((option) => (
            <CheckboxField key={option.key} id={`${id}-${option.key}`} name={field.key} value={option.key} label={option.label} />
          ))}
          {error && <p className="field-error">{error}</p>}
        </fieldset>
      );
    case FormFieldType.Checkbox:
      return <CheckboxField id={id} name={field.key} value="true" label={field.label} hint={hint} />;
    case FormFieldType.Date:
      return (
        <TextField
          id={id}
          name={field.key}
          type="date"
          label={field.label}
          hint={hint}
          error={error}
          required={field.isRequired}
          min={field.dateMin?.slice(0, 10)}
          max={field.dateMax?.slice(0, 10)}
        />
      );
    case FormFieldType.File:
      return (
        <TextField
          id={id}
          name={field.key}
          type="file"
          label={field.label}
          hint={hint}
          error={error}
          required={field.isRequired}
          accept={field.allowedFileTypes.join(',')}
        />
      );
    default:
      // Tanınmayan alan tipi atlanmaz, görünür biçimde raporlanır (gönderim yukarıda engellenir).
      return (
        <p className="form-renderer__unsupported-field">
          Desteklenmeyen alan: {field.label} ({field.type})
        </p>
      );
  }
}
