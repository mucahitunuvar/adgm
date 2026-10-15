import {
  deleteContentItemTranslation,
  getApiErrorMessage,
  listTags,
  setContentItemTranslationTags,
  updateContentItem,
  updateContentItemTranslation,
  type ContentItemDetailResponse,
  type ContentTypeDetailResponse,
  type UpdateContentItemRequest,
  type UpdateContentItemTranslationRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  Button,
  CheckboxField,
  FormSection,
  TextAreaField,
  TextField,
  TranslationTabs,
  UnsavedChangesGuard,
  useToast,
  type TranslationLanguage,
} from '@gm/ui';
import { RichTextEditor } from '@gm/rich-text';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useMediaPicker } from '../../media/useMediaPicker';
import { MediaPickerField } from '../../media/MediaPickerField';

type TranslationForm = Omit<UpdateContentItemTranslationRequest, 'rowVersion'>;

const EMPTY_TRANSLATION: TranslationForm = {
  title: '',
  slug: '',
  summary: '',
  body: '',
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

function toForm(detail: ContentItemDetailResponse, code: string): TranslationForm {
  const existing = detail.translations.find((t) => t.languageCode === code);
  if (!existing) return EMPTY_TRANSLATION;
  return { title: existing.title, slug: existing.slug, summary: existing.summary, body: existing.body, seo: { ...existing.seo } };
}

export function ContentItemGeneralForm({
  contentItemId,
  detail,
  contentType,
  languages,
  defaultLanguageCode,
  onSaved,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  contentType: ContentTypeDetailResponse;
  languages: readonly TranslationLanguage[];
  defaultLanguageCode: string | undefined;
  onSaved: () => void;
}) {
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  return (
    <div>
      <ContentItemCoreForm contentItemId={contentItemId} detail={detail} contentType={contentType} onSaved={onSaved} />
      <FormSection title="Dile göre içerik" description="Her dilin kendi kaydet ve sil düğmesi vardır.">
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(code) => (
            <ContentItemTranslationPanel
              key={code}
              contentItemId={contentItemId}
              languageCode={code}
              rowVersion={detail.rowVersion}
              initial={toForm(detail, code)}
              hasExistingTranslation={detail.translations.some((t) => t.languageCode === code)}
              isDefaultLanguage={code === defaultLanguageCode}
              existingTagIds={detail.translations.find((t) => t.languageCode === code)?.tagIds ?? []}
              onSaved={onSaved}
            />
          )}
        />
      </FormSection>
    </div>
  );
}

function ContentItemCoreForm({
  contentItemId,
  detail,
  contentType,
  onSaved,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  contentType: ContentTypeDetailResponse;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const initial: UpdateContentItemRequest = {
    rowVersion: detail.rowVersion,
    sortOrder: Number(detail.sortOrder),
    isFeatured: detail.isFeatured,
    coverImageMediaId: detail.coverImageMediaId,
    detailImageMediaId: detail.detailImageMediaId,
  };
  const [form, setForm] = useState(initial);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const saveMutation = useMutation({
    mutationFn: () => updateContentItem(client, contentItemId, form),
    onSuccess: () => {
      showToast('İçerik güncellendi.', 'success');
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
      <FormSection title="Genel">
        <TextField
          id="ci-sort-order"
          label="Sıra"
          type="number"
          value={form.sortOrder}
          onChange={(event) => setForm({ ...form, sortOrder: Number(event.target.value) })}
        />
        <CheckboxField
          id="ci-is-featured"
          label="Öne çıkan"
          checked={form.isFeatured}
          onChange={(event) => setForm({ ...form, isFeatured: event.target.checked })}
        />
        {contentType.supportsDetailImage && (
          <>
            <MediaPickerField
              label="Kapak görseli"
              value={form.coverImageMediaId ?? ''}
              previewUrl={detail.coverImageUrl}
              onChange={(id) => setForm({ ...form, coverImageMediaId: id || null })}
            />
            <MediaPickerField
              label="Detay görseli"
              value={form.detailImageMediaId ?? ''}
              previewUrl={detail.detailImageUrl}
              onChange={(id) => setForm({ ...form, detailImageMediaId: id || null })}
            />
          </>
        )}
        <Button type="button" tone="quiet" onClick={() => setForm(initial)} disabled={!isDirty || saveMutation.isPending}>
          Değişiklikleri geri al
        </Button>{' '}
        <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
          {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
        </Button>
      </FormSection>
    </form>
  );
}

function ContentItemTranslationPanel({
  contentItemId,
  languageCode,
  rowVersion,
  initial,
  hasExistingTranslation,
  isDefaultLanguage,
  existingTagIds,
  onSaved,
}: {
  contentItemId: string;
  languageCode: string;
  rowVersion: string;
  initial: TranslationForm;
  hasExistingTranslation: boolean;
  isDefaultLanguage: boolean;
  existingTagIds: readonly string[];
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const { pickMedia, pickerElement } = useMediaPicker();
  const [form, setForm] = useState<TranslationForm>(initial);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  // Yazma ucu (`SetContentItemTranslationTags`) etiket ADI alır, okuma ucu (`translations[].tagIds`)
  // kimlik döner - bu uyumsuzluk yüzünden mevcut adları göstermek için tüm etiketleri çekip
  // kimlik->ad eşlemesi kuruyoruz.
  const { data: tagsPage } = useQuery({
    queryKey: ['website-tags', languageCode],
    queryFn: () => listTags(client, { languageCode, pageSize: 200 }),
  });
  const tagNameById = new Map((tagsPage?.items ?? []).map((tag) => [tag.id, tag.name]));
  const initialTagNames = existingTagIds.map((id) => tagNameById.get(id)).filter((name): name is string => Boolean(name));
  const [tagNamesInput, setTagNamesInput] = useState(initialTagNames.join(', '));
  const isTagsDirty = tagsPage !== undefined && tagNamesInput !== initialTagNames.join(', ');

  const saveMutation = useMutation({
    mutationFn: () => updateContentItemTranslation(client, contentItemId, languageCode, { rowVersion, ...form }),
    onSuccess: () => {
      showToast('İçerik kaydedildi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteContentItemTranslation(client, contentItemId, languageCode, { rowVersion }),
    onSuccess: () => {
      showToast('Çeviri silindi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const saveTagsMutation = useMutation({
    mutationFn: () =>
      setContentItemTranslationTags(client, contentItemId, languageCode, {
        rowVersion,
        tagNames: tagNamesInput
          .split(',')
          .map((name) => name.trim())
          .filter((name) => name.length > 0),
      }),
    onSuccess: () => {
      showToast('Etiketler güncellendi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-tags', languageCode] });
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  async function handleRequestImage() {
    const asset = await pickMedia();
    if (!asset) return null;
    const altText = asset.translations[0]?.altText ?? '';
    return { src: asset.originalUrl, alt: altText };
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    saveMutation.mutate();
  }

  return (
    <div>
      <form onSubmit={handleSubmit}>
        <UnsavedChangesGuard isDirty={isDirty} />
        <TextField id={`ci-title-${languageCode}`} label="Başlık" required value={form.title ?? ''} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} />
        <TextField id={`ci-slug-${languageCode}`} label="Slug" required value={form.slug ?? ''} onChange={(e) => setForm((p) => ({ ...p, slug: e.target.value }))} />
        <TextAreaField id={`ci-summary-${languageCode}`} label="Özet" value={form.summary ?? ''} onChange={(e) => setForm((p) => ({ ...p, summary: e.target.value }))} />
        <div className="form-field">
          <label htmlFor={`ci-body-${languageCode}`}>Gövde</label>
          <RichTextEditor
            value={form.body ?? ''}
            onChange={(html) => setForm((p) => ({ ...p, body: html }))}
            onRequestImage={handleRequestImage}
            ariaLabel="İçerik gövdesi"
          />
        </div>
        <FormSection title="SEO">
          <TextField id={`ci-meta-title-${languageCode}`} label="Meta başlık" value={form.seo?.metaTitle ?? ''} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaTitle: e.target.value } }))} />
          <TextAreaField id={`ci-meta-description-${languageCode}`} label="Meta açıklama" value={form.seo?.metaDescription ?? ''} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaDescription: e.target.value } }))} />
          <TextField id={`ci-canonical-${languageCode}`} label="Canonical URL" value={form.seo?.canonicalUrl ?? ''} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, canonicalUrl: e.target.value || null } }))} />
          <CheckboxField id={`ci-no-index-${languageCode}`} label="Arama motorlarından gizle (noindex)" checked={Boolean(form.seo?.noIndex)} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, noIndex: e.target.checked } }))} />
        </FormSection>
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
      <FormSection title="Etiketler" description="Virgülle ayrılmış etiket adları; var olmayan bir ad yeni etiket olarak oluşturulur.">
        <TextField id={`ci-tags-${languageCode}`} label="Etiketler" value={tagNamesInput} onChange={(e) => setTagNamesInput(e.target.value)} />
        <Button type="button" onClick={() => saveTagsMutation.mutate()} disabled={!isTagsDirty || saveTagsMutation.isPending}>
          {saveTagsMutation.isPending ? 'Kaydediliyor…' : 'Etiketleri kaydet'}
        </Button>
      </FormSection>
      {pickerElement}
    </div>
  );
}
