import {
  deleteContentTypeTranslation,
  getApiErrorMessage,
  updateContentTypeTranslation,
  type ContentTypeDetailResponse,
  type UpdateContentTypeTranslationRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  FormSection,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

type TranslationForm = Omit<UpdateContentTypeTranslationRequest, 'rowVersion'>;

const EMPTY_TRANSLATION: TranslationForm = {
  name: '',
  routePrefix: '',
  seo: {
    metaTitle: '',
    metaDescription: '',
    metaKeywords: '',
    ogTitle: '',
    ogDescription: '',
    ogImageMediaId: null,
    canonicalUrl: null,
    noIndex: false,
  },
};

function toForm(detail: ContentTypeDetailResponse, code: string): TranslationForm {
  const existing = detail.translations.find((t) => t.languageCode === code);
  if (!existing) return EMPTY_TRANSLATION;
  return { name: existing.name, routePrefix: existing.routePrefix, seo: { ...existing.seo } };
}

export function ContentTypeTranslationForm({
  contentTypeId,
  detail,
  languages,
  defaultLanguageCode,
  onSaved,
}: {
  contentTypeId: string;
  detail: ContentTypeDetailResponse;
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
          <ContentTypeTranslationPanel
            key={code}
            contentTypeId={contentTypeId}
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

function ContentTypeTranslationPanel({
  contentTypeId,
  languageCode,
  rowVersion,
  initial,
  hasExistingTranslation,
  isDefaultLanguage,
  onSaved,
}: {
  contentTypeId: string;
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
    mutationFn: () => updateContentTypeTranslation(client, contentTypeId, languageCode, { rowVersion, ...form }),
    onSuccess: () => {
      showToast('Çeviri kaydedildi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteContentTypeTranslation(client, contentTypeId, languageCode, { rowVersion }),
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
      <TextField id={`ctt-name-${languageCode}`} label="Ad" required value={form.name ?? ''} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} />
      <TextField
        id={`ctt-route-${languageCode}`}
        label="Rota öneki"
        required
        value={form.routePrefix ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, routePrefix: e.target.value }))}
      />
      <TextField
        id={`ctt-meta-title-${languageCode}`}
        label="Meta başlık"
        value={form.seo?.metaTitle ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaTitle: e.target.value } }))}
      />
      <TextField
        id={`ctt-meta-description-${languageCode}`}
        label="Meta açıklama"
        value={form.seo?.metaDescription ?? ''}
        onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaDescription: e.target.value } }))}
      />
      <CheckboxField
        id={`ctt-no-index-${languageCode}`}
        label="Arama motorlarından gizle (noindex)"
        checked={Boolean(form.seo?.noIndex)}
        onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, noIndex: e.target.checked } }))}
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
