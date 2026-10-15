import {
  ContentSchemaKind,
  ContentTypeSortMode,
  getApiErrorMessage,
  updateContentType,
  type ContentTypeDetailResponse,
  type UpdateContentTypeRequest,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, CheckboxField, FieldRow, FormSection, SelectField, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';

const sortModeOptions = enumOptions(ContentTypeSortMode, 'ContentTypeSortMode');
const schemaKindOptions = enumOptions(ContentSchemaKind, 'ContentSchemaKind');

type FormState = Omit<UpdateContentTypeRequest, 'rowVersion'>;

function toForm(detail: ContentTypeDetailResponse): FormState {
  return {
    listTemplate: detail.listTemplate,
    detailTemplate: detail.detailTemplate,
    sortMode: detail.sortMode,
    sortOrder: detail.sortOrder,
    supportsHierarchy: detail.supportsHierarchy,
    supportsCategories: detail.supportsCategories,
    supportsTags: detail.supportsTags,
    supportsDetailImage: detail.supportsDetailImage,
    supportsGallery: detail.supportsGallery,
    supportsVideos: detail.supportsVideos,
    supportsAttachments: detail.supportsAttachments,
    supportsEvent: detail.supportsEvent,
    supportsBlockLayout: detail.supportsBlockLayout,
    supportsForm: detail.supportsForm,
    supportsRelatedContent: detail.supportsRelatedContent,
    hasDetailPage: detail.hasDetailPage,
    hasListingPage: detail.hasListingPage,
    isSearchable: detail.isSearchable,
    requiresReview: detail.requiresReview,
    schemaKind: detail.schemaKind,
  };
}

export function ContentTypeGeneralForm({ detail, onSaved }: { detail: ContentTypeDetailResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState<FormState>(() => toForm(detail));
  const initial = toForm(detail);
  const isDirty = JSON.stringify(form) !== JSON.stringify(initial);

  const mutation = useMutation({
    mutationFn: () => updateContentType(client, detail.id, { rowVersion: detail.rowVersion, ...form }),
    onSuccess: () => {
      showToast('İçerik tipi güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate();
  }

  function toggle(field: keyof FormState) {
    setForm((prev) => ({ ...prev, [field]: !prev[field] }));
  }

  return (
    <form onSubmit={handleSubmit}>
      <UnsavedChangesGuard isDirty={isDirty} />
      <FormSection title="Genel" description={`Anahtar: ${detail.key} (değiştirilemez).`}>
        <FieldRow>
          <TextField id="ctg-list-template" label="Liste şablonu" required value={form.listTemplate ?? ''} onChange={(e) => setForm((p) => ({ ...p, listTemplate: e.target.value }))} />
          <TextField id="ctg-detail-template" label="Detay şablonu" required value={form.detailTemplate ?? ''} onChange={(e) => setForm((p) => ({ ...p, detailTemplate: e.target.value }))} />
        </FieldRow>
        <FieldRow>
          <SelectField id="ctg-sort-mode" label="İçerik sıralama modu" value={form.sortMode ?? ''} onChange={(e) => setForm((p) => ({ ...p, sortMode: e.target.value }))}>
            {sortModeOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </SelectField>
          <SelectField id="ctg-schema-kind" label="İçerik şeması" value={form.schemaKind ?? ''} onChange={(e) => setForm((p) => ({ ...p, schemaKind: e.target.value }))}>
            {schemaKindOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </SelectField>
          <TextField id="ctg-sort-order" label="Sıra" type="number" required value={form.sortOrder} onChange={(e) => setForm((p) => ({ ...p, sortOrder: Number(e.target.value) }))} />
        </FieldRow>
      </FormSection>
      <FormSection title="Desteklenen özellikler">
        <CheckboxField id="ctg-has-listing" label="Liste sayfası var" checked={Boolean(form.hasListingPage)} onChange={() => toggle('hasListingPage')} />
        <CheckboxField id="ctg-has-detail" label="Detay sayfası var" checked={Boolean(form.hasDetailPage)} onChange={() => toggle('hasDetailPage')} />
        <CheckboxField id="ctg-searchable" label="Genel aramada yer alır" checked={Boolean(form.isSearchable)} onChange={() => toggle('isSearchable')} />
        <CheckboxField id="ctg-requires-review" label="Yayın öncesi inceleme gerektirir" checked={Boolean(form.requiresReview)} onChange={() => toggle('requiresReview')} />
        <CheckboxField id="ctg-hierarchy" label="Üst/alt içerik hiyerarşisi" checked={Boolean(form.supportsHierarchy)} onChange={() => toggle('supportsHierarchy')} />
        <CheckboxField id="ctg-categories" label="Kategoriler" checked={Boolean(form.supportsCategories)} onChange={() => toggle('supportsCategories')} />
        <CheckboxField id="ctg-tags" label="Etiketler" checked={Boolean(form.supportsTags)} onChange={() => toggle('supportsTags')} />
        <CheckboxField id="ctg-detail-image" label="Detay görseli" checked={Boolean(form.supportsDetailImage)} onChange={() => toggle('supportsDetailImage')} />
        <CheckboxField id="ctg-gallery" label="Galeri" checked={Boolean(form.supportsGallery)} onChange={() => toggle('supportsGallery')} />
        <CheckboxField id="ctg-videos" label="Video listesi" checked={Boolean(form.supportsVideos)} onChange={() => toggle('supportsVideos')} />
        <CheckboxField id="ctg-attachments" label="Ek dosyalar" checked={Boolean(form.supportsAttachments)} onChange={() => toggle('supportsAttachments')} />
        <CheckboxField id="ctg-event" label="Etkinlik alanları" checked={Boolean(form.supportsEvent)} onChange={() => toggle('supportsEvent')} />
        <CheckboxField id="ctg-block-layout" label="Blok tabanlı düzen" checked={Boolean(form.supportsBlockLayout)} onChange={() => toggle('supportsBlockLayout')} />
        <CheckboxField id="ctg-form" label="Form eki" checked={Boolean(form.supportsForm)} onChange={() => toggle('supportsForm')} />
        <CheckboxField id="ctg-related" label="İlişkili içerik" checked={Boolean(form.supportsRelatedContent)} onChange={() => toggle('supportsRelatedContent')} />
      </FormSection>
      <Button type="button" tone="quiet" onClick={() => setForm(initial)} disabled={!isDirty || mutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || mutation.isPending}>
        {mutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
    </form>
  );
}
