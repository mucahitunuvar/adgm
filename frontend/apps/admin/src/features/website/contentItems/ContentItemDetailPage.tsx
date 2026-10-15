import { getApiErrorMessage, getContentItemById, updateContentItemTranslation, type ContentItemTranslationResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, buttonClassName, CheckboxField, ErrorState, FormSection, LoadingState, PageHeader, TextAreaField, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { RichTextEditor } from '@gm/rich-text';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMediaPicker } from '../../media/useMediaPicker';
import { ROUTES } from '../../../routes';

type TranslationForm = Pick<ContentItemTranslationResponse, 'title' | 'slug' | 'summary' | 'body' | 'seo'>;

function toForm(translation: ContentItemTranslationResponse): TranslationForm {
  return { title: translation.title, slug: translation.slug, summary: translation.summary, body: translation.body, seo: translation.seo };
}

export function ContentItemDetailPage() {
  const { contentItemId } = useParams<{ contentItemId: string }>();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const { pickMedia, pickerElement } = useMediaPicker();

  const queryKey = ['website-content-item', contentItemId] as const;
  const { data: detail, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getContentItemById(client, contentItemId!),
    enabled: Boolean(contentItemId),
  });

  const translation = detail?.translations[0];
  const [form, setForm] = useState<TranslationForm | null>(null);
  const activeForm = form ?? (translation ? toForm(translation) : null);
  const isDirty = translation ? JSON.stringify(activeForm) !== JSON.stringify(toForm(translation)) : false;

  const saveMutation = useMutation({
    mutationFn: () =>
      updateContentItemTranslation(client, contentItemId!, translation!.languageCode, {
        rowVersion: detail!.rowVersion,
        ...activeForm!,
      }),
    onSuccess: () => {
      showToast('İçerik kaydedildi.', 'success');
      setForm(null);
      void queryClient.invalidateQueries({ queryKey });
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  async function handleRequestImage() {
    const asset = await pickMedia();
    if (!asset) return null;
    const altText = asset.translations[0]?.altText ?? '';
    return { src: asset.originalUrl, alt: altText };
  }

  if (!contentItemId) return <ErrorState description="İçerik kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !detail || !translation || !activeForm) {
    return <ErrorState description={error ? getApiErrorMessage(error) : 'İçerik bulunamadı.'} />;
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    saveMutation.mutate();
  }

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title={translation.title}
        description={`Yol: ${translation.fullPath}`}
        actions={
          <Link to={ROUTES.websiteContentItems} className={buttonClassName('quiet')}>
            Listeye dön
          </Link>
        }
      />
      <form onSubmit={handleSubmit}>
        <UnsavedChangesGuard isDirty={isDirty} />
        <FormSection title="Genel">
          <TextField
            id="ci-title"
            label="Başlık"
            required
            value={activeForm.title}
            onChange={(event) => setForm({ ...activeForm, title: event.target.value })}
          />
          <TextField id="ci-slug" label="Slug" required value={activeForm.slug} onChange={(event) => setForm({ ...activeForm, slug: event.target.value })} />
          <TextAreaField
            id="ci-summary"
            label="Özet"
            value={activeForm.summary}
            onChange={(event) => setForm({ ...activeForm, summary: event.target.value })}
          />
          <div className="form-field">
            <label htmlFor="ci-body">Gövde</label>
            <RichTextEditor
              value={activeForm.body}
              onChange={(html) => setForm({ ...activeForm, body: html })}
              onRequestImage={handleRequestImage}
              ariaLabel="İçerik gövdesi"
            />
          </div>
        </FormSection>
        <FormSection title="SEO">
          <TextField
            id="ci-meta-title"
            label="Meta başlık"
            value={activeForm.seo.metaTitle}
            onChange={(event) => setForm({ ...activeForm, seo: { ...activeForm.seo, metaTitle: event.target.value } })}
          />
          <TextAreaField
            id="ci-meta-description"
            label="Meta açıklama"
            value={activeForm.seo.metaDescription}
            onChange={(event) => setForm({ ...activeForm, seo: { ...activeForm.seo, metaDescription: event.target.value } })}
          />
          <TextField
            id="ci-canonical-url"
            label="Canonical URL"
            value={activeForm.seo.canonicalUrl ?? ''}
            onChange={(event) => setForm({ ...activeForm, seo: { ...activeForm.seo, canonicalUrl: event.target.value || null } })}
          />
          <CheckboxField
            id="ci-no-index"
            label="Arama motorlarından gizle (noindex)"
            checked={activeForm.seo.noIndex}
            onChange={(event) => setForm({ ...activeForm, seo: { ...activeForm.seo, noIndex: event.target.checked } })}
          />
        </FormSection>
        <Button type="button" tone="quiet" onClick={() => setForm(null)} disabled={!isDirty || saveMutation.isPending}>
          Değişiklikleri geri al
        </Button>{' '}
        <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
          {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
        </Button>
      </form>
      {pickerElement}
    </div>
  );
}

export default ContentItemDetailPage;
