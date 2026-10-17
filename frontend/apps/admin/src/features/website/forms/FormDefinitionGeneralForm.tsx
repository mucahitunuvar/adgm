import {
  getApiErrorMessage,
  LegalDocumentKind,
  updateFormDefinition,
  type FormDefinitionDetailResponse,
  type UpdateFormDefinitionExplicitConsentInput,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, FormSection, TextAreaField, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { LegalDocumentSelector } from '../shared/LegalDocumentSelector';

interface GeneralForm {
  retentionDays: number | null;
  emailsText: string;
  privacyNoticeKey: string | null;
  explicitConsents: UpdateFormDefinitionExplicitConsentInput[];
}

function toForm(detail: FormDefinitionDetailResponse): GeneralForm {
  return {
    retentionDays: typeof detail.retentionDays === 'number' ? detail.retentionDays : Number(detail.retentionDays),
    emailsText: detail.notificationEmails.join('\n'),
    privacyNoticeKey: detail.privacyNoticeKey || null,
    explicitConsents: detail.explicitConsents.map((c) => ({ legalDocumentKey: c.legalDocumentKey, isRequired: c.isRequired })),
  };
}

export function FormDefinitionGeneralForm({ formId, detail, onSaved }: { formId: string; detail: FormDefinitionDetailResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<GeneralForm>(() => toForm(detail));
  const [loadedRowVersion, setLoadedRowVersion] = useState(detail.rowVersion);

  if (detail.rowVersion !== loadedRowVersion) {
    setLoadedRowVersion(detail.rowVersion);
    setForm(toForm(detail));
  }

  const initial = toForm(detail);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const saveMutation = useMutation({
    mutationFn: () =>
      updateFormDefinition(client, formId, {
        rowVersion: detail.rowVersion,
        retentionDays: form.retentionDays,
        notificationEmails: parseEmailLines(form.emailsText),
        privacyNoticeKey: form.privacyNoticeKey,
        explicitConsents: form.explicitConsents,
      }),
    onSuccess: () => {
      showToast('Form ayarları kaydedildi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    saveMutation.mutate();
  }

  function updateConsent(index: number, patch: Partial<UpdateFormDefinitionExplicitConsentInput>) {
    setForm((prev) => ({
      ...prev,
      explicitConsents: prev.explicitConsents.map((consent, i) => (i === index ? { ...consent, ...patch } : consent)),
    }));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="Anahtar" description="Form oluşturulduktan sonra değiştirilemez.">
        <TextField id="form-key-readonly" label="Anahtar" value={detail.key} readOnly disabled />
      </FormSection>
      <FormSection title="Aydınlatma metni" description="Yayınlı sürümü olmayan bir anahtar seçilirse form public sitede kapalı kalır.">
        <LegalDocumentSelector
          idPrefix="form-general-privacy-notice"
          label="Aydınlatma metni"
          kind={LegalDocumentKind.PrivacyNotice}
          allowEmpty={false}
          value={form.privacyNoticeKey}
          onChange={(next) => setForm((prev) => ({ ...prev, privacyNoticeKey: next }))}
        />
      </FormSection>
      <FormSection title="Açık rıza metinleri" description="Başvuru sırasında ayrıca onaylatılacak açık rıza metinleri (ör. özel nitelikli veri için).">
        {form.explicitConsents.length === 0 && <p className="field-hint">Henüz açık rıza metni eklenmemiş.</p>}
        {form.explicitConsents.map((consent, index) => (
          <div key={index} className="field-row" style={{ alignItems: 'flex-end' }}>
            <LegalDocumentSelector
              idPrefix={`form-explicit-consent-${index}`}
              label="Açık rıza metni"
              kind={LegalDocumentKind.ExplicitConsent}
              allowEmpty={false}
              value={consent.legalDocumentKey}
              onChange={(next) => updateConsent(index, { legalDocumentKey: next })}
            />
            <CheckboxField
              id={`form-explicit-consent-required-${index}`}
              label="Zorunlu"
              checked={consent.isRequired}
              onChange={(event) => updateConsent(index, { isRequired: event.target.checked })}
            />
            <Button
              type="button"
              tone="danger"
              onClick={() => setForm((prev) => ({ ...prev, explicitConsents: prev.explicitConsents.filter((_, i) => i !== index) }))}
            >
              Kaldır
            </Button>
          </div>
        ))}
        <Button
          type="button"
          tone="secondary"
          onClick={() => setForm((prev) => ({ ...prev, explicitConsents: [...prev.explicitConsents, { legalDocumentKey: null, isRequired: true }] }))}
        >
          Açık rıza metni ekle
        </Button>
      </FormSection>
      <FormSection title="Bildirim ve saklama" description="Yeni başvuru geldiğinde bilgilendirilecek e-posta adresleri (başvuru içeriği e-posta gövdesine yazılmaz).">
        <TextAreaField
          id="form-general-notification-emails"
          label="Bildirim e-postaları"
          hint="Her satıra bir e-posta."
          value={form.emailsText}
          onChange={(event) => setForm((prev) => ({ ...prev, emailsText: event.target.value }))}
        />
        <TextField
          id="form-general-retention-days"
          label="Saklama süresi (gün)"
          type="number"
          min={1}
          value={form.retentionDays ?? ''}
          onChange={(event) => setForm((prev) => ({ ...prev, retentionDays: event.target.value === '' ? null : Number(event.target.value) }))}
        />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setForm(initial)} disabled={!isDirty || saveMutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}

function parseEmailLines(text: string): string[] {
  return text
    .split(/[\n,]/)
    .map((line) => line.trim())
    .filter((line) => line !== '');
}
