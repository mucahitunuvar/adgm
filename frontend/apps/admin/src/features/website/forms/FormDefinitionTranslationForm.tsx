import {
  deleteFormDefinitionTranslation,
  getApiErrorMessage,
  updateFormDefinitionTranslation,
  type FormDefinitionDetailResponse,
  type UpdateFormDefinitionTranslationRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, TextAreaField, TextField, TranslationTabs, UnsavedChangesGuard, useToast, type TranslationLanguage } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

type TranslationForm = Omit<UpdateFormDefinitionTranslationRequest, 'rowVersion'>;

const EMPTY_TRANSLATION: TranslationForm = { title: '', description: '', successMessage: '', submitButtonLabel: '' };

function toForm(detail: FormDefinitionDetailResponse, code: string): TranslationForm {
  const existing = detail.translations.find((t) => t.languageCode === code);
  if (!existing) return EMPTY_TRANSLATION;
  return {
    title: existing.title,
    description: existing.description,
    successMessage: existing.successMessage,
    submitButtonLabel: existing.submitButtonLabel,
  };
}

export function FormDefinitionTranslationForm({
  formId,
  detail,
  languages,
  defaultLanguageCode,
  onSaved,
}: {
  formId: string;
  detail: FormDefinitionDetailResponse;
  languages: readonly TranslationLanguage[];
  defaultLanguageCode: string | undefined;
  onSaved: () => void;
}) {
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  return (
    <FormSection title="Dile göre çeviriler" description="Her dilin kendi kaydet ve sil düğmesi vardır.">
      <TranslationTabs
        languages={languages}
        activeCode={activeLanguage}
        onChange={setActiveLanguage}
        renderPanel={(code) => (
          <FormDefinitionTranslationPanel
            key={code}
            formId={formId}
            languageCode={code}
            rowVersion={detail.rowVersion}
            initial={toForm(detail, code)}
            hasExistingTranslation={detail.translations.some((t) => t.languageCode === code)}
            isDefaultLanguage={code === defaultLanguageCode}
            onSaved={onSaved}
          />
        )}
      />
    </FormSection>
  );
}

function FormDefinitionTranslationPanel({
  formId,
  languageCode,
  rowVersion,
  initial,
  hasExistingTranslation,
  isDefaultLanguage,
  onSaved,
}: {
  formId: string;
  languageCode: string;
  rowVersion: string;
  initial: TranslationForm;
  hasExistingTranslation: boolean;
  isDefaultLanguage: boolean;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<TranslationForm>(initial);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const saveMutation = useMutation({
    mutationFn: () => updateFormDefinitionTranslation(client, formId, languageCode, { rowVersion, ...form }),
    onSuccess: () => {
      showToast('Çeviri kaydedildi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteFormDefinitionTranslation(client, formId, languageCode, { rowVersion }),
    onSuccess: () => {
      showToast('Çeviri silindi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    saveMutation.mutate();
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <TextField id={`fdt-title-${languageCode}`} label="Başlık" required value={form.title ?? ''} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} />
      <TextAreaField
        id={`fdt-description-${languageCode}`}
        label="Açıklama"
        value={form.description ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
      />
      <TextField
        id={`fdt-success-${languageCode}`}
        label="Başarı mesajı"
        required
        value={form.successMessage ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, successMessage: e.target.value }))}
      />
      <TextField
        id={`fdt-submit-label-${languageCode}`}
        label="Gönder düğmesi metni"
        required
        value={form.submitButtonLabel ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, submitButtonLabel: e.target.value }))}
      />
      <Button type="button" tone="quiet" onClick={() => setForm(initial)} disabled={!isDirty || saveMutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>{' '}
      {hasExistingTranslation && !isDefaultLanguage && (
        <Button type="button" tone="danger" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
          {deleteMutation.isPending ? 'Siliniyor…' : 'Bu dildeki çeviriyi sil'}
        </Button>
      )}
    </form>
  );
}
