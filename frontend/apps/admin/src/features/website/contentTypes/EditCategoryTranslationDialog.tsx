import {
  getApiErrorMessage,
  getContentCategoryById,
  updateContentCategoryTranslation,
  type UpdateContentCategoryTranslationRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, CheckboxField, ErrorState, LoadingState, TextField, TranslationTabs, useToast, type TranslationLanguage } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

type TranslationForm = Omit<UpdateContentCategoryTranslationRequest, 'rowVersion'>;

const EMPTY: TranslationForm = {
  name: '',
  slug: '',
  seo: { metaTitle: '', metaDescription: '', metaKeywords: '', ogTitle: '', ogDescription: '', ogImageMediaId: null, canonicalUrl: null, noIndex: false },
};

/**
 * `GetContentCategoryById` eklenmeden önce bu ekran yoktu (bkz. ContentCategoriesPanel'deki eski
 * not) - körlemesine yazmak mevcut SEO verisinin üzerine boş değer yazabileceği için. Artık önce
 * okunup üzerine yazılıyor.
 */
export function EditCategoryTranslationDialog({
  open,
  contentTypeId,
  categoryId,
  languages,
  onClose,
}: {
  open: boolean;
  contentTypeId: string;
  categoryId: string | null;
  languages: readonly TranslationLanguage[];
  onClose: () => void;
}) {
  const { client } = useAuth();
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  const { data: category, isLoading, error } = useQuery({
    queryKey: ['website-content-category', contentTypeId, categoryId],
    queryFn: () => getContentCategoryById(client, contentTypeId, categoryId!),
    enabled: open && Boolean(categoryId),
  });

  if (!open || !categoryId) return null;

  return (
    <AccessibleDialog open={open} onClose={onClose} title="Kategori çevirisi ve SEO">
      {isLoading && <LoadingState />}
      {error && <ErrorState description={getApiErrorMessage(error)} />}
      {category && (
        <TranslationTabs
          languages={languages}
          activeCode={activeLanguage}
          onChange={setActiveLanguage}
          renderPanel={(code) => {
            const existing = category.translations.find((t) => t.languageCode === code);
            const initial: TranslationForm = existing ? { name: existing.name, slug: existing.slug, seo: { ...existing.seo } } : EMPTY;
            return (
              <CategoryTranslationForm
                key={code}
                contentTypeId={contentTypeId}
                categoryId={categoryId}
                languageCode={code}
                rowVersion={category.rowVersion}
                initial={initial}
                onSaved={onClose}
              />
            );
          }}
        />
      )}
    </AccessibleDialog>
  );
}

function CategoryTranslationForm({
  contentTypeId,
  categoryId,
  languageCode,
  rowVersion,
  initial,
  onSaved,
}: {
  contentTypeId: string;
  categoryId: string;
  languageCode: string;
  rowVersion: string;
  initial: TranslationForm;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [form, setForm] = useState<TranslationForm>(initial);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const saveMutation = useMutation({
    mutationFn: () => updateContentCategoryTranslation(client, contentTypeId, categoryId, languageCode, { rowVersion, ...form }),
    onSuccess: () => {
      showToast('Kategori çevirisi kaydedildi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-content-categories', contentTypeId] });
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
      <TextField id={`cat-name-${languageCode}`} label="Ad" required value={form.name ?? ''} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} />
      <TextField id={`cat-slug-${languageCode}`} label="Slug" required value={form.slug ?? ''} onChange={(e) => setForm((p) => ({ ...p, slug: e.target.value }))} />
      <TextField id={`cat-meta-title-${languageCode}`} label="Meta başlık" value={form.seo?.metaTitle ?? ''} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaTitle: e.target.value } }))} />
      <TextField id={`cat-meta-description-${languageCode}`} label="Meta açıklama" value={form.seo?.metaDescription ?? ''} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, metaDescription: e.target.value } }))} />
      <CheckboxField id={`cat-no-index-${languageCode}`} label="Arama motorlarından gizle (noindex)" checked={Boolean(form.seo?.noIndex)} onChange={(e) => setForm((p) => ({ ...p, seo: { ...p.seo!, noIndex: e.target.checked } }))} />
      <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
