import { ContentSchemaKind, ContentTypeSortMode, createContentType, getApiErrorMessage, type CreateContentTypeRequest } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, CheckboxField, FieldRow, FormSection, SelectField, TextField, useToast } from '@gm/ui';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { enumOptions } from '../../../i18n/enumOptions';
import { ROUTES } from '../../../routes';

const sortModeOptions = enumOptions(ContentTypeSortMode, 'ContentTypeSortMode');
const schemaKindOptions = enumOptions(ContentSchemaKind, 'ContentSchemaKind');

const EMPTY_FORM: CreateContentTypeRequest = {
  key: '',
  listTemplate: '',
  detailTemplate: '',
  sortMode: ContentTypeSortMode.Manual,
  sortOrder: 0,
  supportsHierarchy: false,
  supportsCategories: false,
  supportsTags: false,
  supportsDetailImage: true,
  supportsGallery: false,
  supportsVideos: false,
  supportsAttachments: false,
  supportsEvent: false,
  supportsBlockLayout: false,
  supportsForm: false,
  supportsRelatedContent: false,
  hasDetailPage: true,
  hasListingPage: true,
  isSearchable: true,
  requiresReview: false,
  defaultLanguageName: '',
  defaultLanguageRoutePrefix: '',
  schemaKind: ContentSchemaKind.None,
  seo: {
    metaTitle: null,
    metaDescription: null,
    metaKeywords: null,
    ogTitle: null,
    ogDescription: null,
    ogImageMediaId: null,
    canonicalUrl: null,
    noIndex: false,
  },
};

export function CreateContentTypeDialog({ open, onCancel }: { open: boolean; onCancel: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateContentTypeRequest>(EMPTY_FORM);

  const mutation = useMutation({
    mutationFn: () => createContentType(client, form),
    onSuccess: (response) => {
      showToast('İçerik tipi oluşturuldu.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-content-types'] });
      setForm(EMPTY_FORM);
      navigate(ROUTES.websiteContentTypeDetail(response.id));
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  if (!open) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function toggle(field: keyof CreateContentTypeRequest) {
    setForm((prev) => ({ ...prev, [field]: !prev[field] }));
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yeni içerik tipi" className="accessible-dialog--wide">
      <h2>Yeni içerik tipi</h2>
      <form onSubmit={handleSubmit}>
        <FormSection title="Temel bilgiler" description="Anahtar oluşturulduktan sonra değiştirilemez (rota ve şema kararlılığı).">
          <FieldRow>
            <TextField id="ct-key" label="Anahtar" hint="ör. news, event — küçük harf, boşluksuz." required value={form.key ?? ''} onChange={(e) => setForm((p) => ({ ...p, key: e.target.value }))} />
            <TextField
              id="ct-default-name"
              label="Ad (varsayılan dil)"
              required
              value={form.defaultLanguageName ?? ''}
              onChange={(e) => setForm((p) => ({ ...p, defaultLanguageName: e.target.value }))}
            />
          </FieldRow>
          <FieldRow>
            <TextField
              id="ct-route-prefix"
              label="Rota öneki (varsayılan dil)"
              hint="ör. haberler"
              required
              value={form.defaultLanguageRoutePrefix ?? ''}
              onChange={(e) => setForm((p) => ({ ...p, defaultLanguageRoutePrefix: e.target.value }))}
            />
            <TextField id="ct-sort-order" label="Sıra" type="number" required value={form.sortOrder} onChange={(e) => setForm((p) => ({ ...p, sortOrder: Number(e.target.value) }))} />
          </FieldRow>
          <FieldRow>
            <TextField
              id="ct-list-template"
              label="Liste şablonu"
              required
              value={form.listTemplate ?? ''}
              onChange={(e) => setForm((p) => ({ ...p, listTemplate: e.target.value }))}
            />
            <TextField
              id="ct-detail-template"
              label="Detay şablonu"
              required
              value={form.detailTemplate ?? ''}
              onChange={(e) => setForm((p) => ({ ...p, detailTemplate: e.target.value }))}
            />
          </FieldRow>
          <FieldRow>
            <SelectField id="ct-sort-mode" label="İçerik sıralama modu" value={form.sortMode ?? ''} onChange={(e) => setForm((p) => ({ ...p, sortMode: e.target.value }))}>
              {sortModeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </SelectField>
            <SelectField id="ct-schema-kind" label="İçerik şeması" value={form.schemaKind ?? ''} onChange={(e) => setForm((p) => ({ ...p, schemaKind: e.target.value }))}>
              {schemaKindOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </SelectField>
          </FieldRow>
        </FormSection>
        <FormSection title="Desteklenen özellikler" description="Bu bayraklar içerik formunda hangi alanların görüneceğini belirler.">
          <CheckboxField id="ct-has-listing" label="Liste sayfası var" checked={form.hasListingPage} onChange={() => toggle('hasListingPage')} />
          <CheckboxField id="ct-has-detail" label="Detay sayfası var" checked={form.hasDetailPage} onChange={() => toggle('hasDetailPage')} />
          <CheckboxField id="ct-searchable" label="Genel aramada yer alır" checked={form.isSearchable} onChange={() => toggle('isSearchable')} />
          <CheckboxField id="ct-requires-review" label="Yayın öncesi inceleme gerektirir" checked={form.requiresReview} onChange={() => toggle('requiresReview')} />
          <CheckboxField id="ct-hierarchy" label="Üst/alt içerik hiyerarşisi" checked={form.supportsHierarchy} onChange={() => toggle('supportsHierarchy')} />
          <CheckboxField id="ct-categories" label="Kategoriler" checked={form.supportsCategories} onChange={() => toggle('supportsCategories')} />
          <CheckboxField id="ct-tags" label="Etiketler" checked={form.supportsTags} onChange={() => toggle('supportsTags')} />
          <CheckboxField id="ct-detail-image" label="Detay görseli" checked={form.supportsDetailImage} onChange={() => toggle('supportsDetailImage')} />
          <CheckboxField id="ct-gallery" label="Galeri" checked={form.supportsGallery} onChange={() => toggle('supportsGallery')} />
          <CheckboxField id="ct-videos" label="Video listesi" checked={form.supportsVideos} onChange={() => toggle('supportsVideos')} />
          <CheckboxField id="ct-attachments" label="Ek dosyalar" checked={form.supportsAttachments} onChange={() => toggle('supportsAttachments')} />
          <CheckboxField id="ct-event" label="Etkinlik alanları" checked={form.supportsEvent} onChange={() => toggle('supportsEvent')} />
          <CheckboxField id="ct-block-layout" label="Blok tabanlı düzen" checked={form.supportsBlockLayout} onChange={() => toggle('supportsBlockLayout')} />
          <CheckboxField id="ct-form" label="Form eki" checked={form.supportsForm} onChange={() => toggle('supportsForm')} />
          <CheckboxField id="ct-related" label="İlişkili içerik" checked={form.supportsRelatedContent} onChange={() => toggle('supportsRelatedContent')} />
        </FormSection>
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={mutation.isPending}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
