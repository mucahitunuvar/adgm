import { createContentItem, getApiErrorMessage, getContentTypes, type CreateContentItemRequest } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, SelectField, TextField, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../routes';

const EMPTY_SEO: CreateContentItemRequest['seo'] = {
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  ogTitle: null,
  ogDescription: null,
  ogImageMediaId: null,
  canonicalUrl: null,
  noIndex: false,
};

export function CreateContentItemDialog({ open, onCancel }: { open: boolean; onCancel: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [contentTypeId, setContentTypeId] = useState('');
  const [title, setTitle] = useState('');
  const [slug, setSlug] = useState('');

  const { data: contentTypes } = useQuery({ queryKey: ['website-content-types'], queryFn: () => getContentTypes(client), enabled: open });

  const mutation = useMutation({
    mutationFn: () =>
      createContentItem(client, {
        contentTypeId,
        parentId: null,
        sortOrder: 0,
        isFeatured: false,
        coverImageMediaId: null,
        detailImageMediaId: null,
        defaultLanguageTitle: title,
        defaultLanguageSlug: slug || null,
        defaultLanguageSummary: null,
        defaultLanguageBody: null,
        seo: EMPTY_SEO,
      }),
    onSuccess: (response) => {
      showToast('İçerik oluşturuldu.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-content-items'] });
      setTitle('');
      setSlug('');
      setContentTypeId('');
      navigate(ROUTES.websiteContentItemDetail(response.id));
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  if (!open) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yeni içerik">
      <form onSubmit={handleSubmit}>
        <SelectField id="ci-content-type" label="İçerik tipi" required value={contentTypeId} onChange={(event) => setContentTypeId(event.target.value)}>
          <option value="" disabled>
            Seçiniz
          </option>
          {(contentTypes ?? []).map((type) => (
            <option key={type.id} value={type.id}>
              {type.defaultLanguageName}
            </option>
          ))}
        </SelectField>
        <TextField id="ci-title" label="Başlık (varsayılan dil)" required value={title} onChange={(event) => setTitle(event.target.value)} />
        <TextField id="ci-slug" label="Slug" hint="Boş bırakılırsa başlıktan üretilir." value={slug} onChange={(event) => setSlug(event.target.value)} />
        <Button type="button" tone="quiet" onClick={onCancel} disabled={mutation.isPending}>
          Vazgeç
        </Button>{' '}
        <Button type="submit" disabled={!contentTypeId || !title || mutation.isPending}>
          {mutation.isPending ? 'Oluşturuluyor…' : 'Oluştur'}
        </Button>
      </form>
    </AccessibleDialog>
  );
}
