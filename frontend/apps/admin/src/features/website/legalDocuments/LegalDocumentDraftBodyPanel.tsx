import { getApiErrorMessage, updateLegalDocumentDraftBody } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { RichTextEditor } from '@gm/rich-text';
import { Button, SafeHtml, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useMediaPicker } from '../../media/useMediaPicker';

/** Çağıran taraf `key={languageCode}` vermelidir: dil değişince tazelenmiş durumla yeniden kurulur. */
export function LegalDocumentDraftBodyPanel({
  documentId,
  languageCode,
  rowVersion,
  /** Son kaydedilen (backend'in sanitize ettiği) taslak gövde - önizleme bundan okunur, editörün kendi çıktısından değil. */
  savedBody,
  onSaved,
}: {
  documentId: string;
  languageCode: string;
  rowVersion: string;
  savedBody: string;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const { pickMedia, pickerElement } = useMediaPicker();
  const [body, setBody] = useState(savedBody);
  const [showPreview, setShowPreview] = useState(false);
  const isDirty = body !== savedBody;

  const saveMutation = useMutation({
    mutationFn: () => updateLegalDocumentDraftBody(client, documentId, languageCode, { rowVersion, body }),
    onSuccess: () => {
      showToast('Taslak gövde kaydedildi.', 'success');
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

  return (
    <div>
      <UnsavedChangesGuard isDirty={isDirty} />
      <div className="form-field">
        <label htmlFor={`legal-document-draft-body-${languageCode}`}>Gövde</label>
        <RichTextEditor value={body} onChange={setBody} onRequestImage={handleRequestImage} ariaLabel="Yasal metin gövdesi" />
      </div>
      <Button type="button" tone="quiet" onClick={() => setBody(savedBody)} disabled={!isDirty || saveMutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Taslağı kaydet'}
      </Button>{' '}
      <Button type="button" tone="quiet" onClick={() => setShowPreview((current) => !current)} disabled={isDirty}>
        {showPreview ? 'Önizlemeyi gizle' : 'Önizle'}
      </Button>
      {isDirty && <p className="field-hint">Önizleme yalnız son kaydedilen biçimi gösterir; önce kaydedin.</p>}
      {showPreview && !isDirty && (
        <div className="form-field">
          <p className="field-hint">Önizleme (backend tarafından temizlenmiş gerçek çıktı):</p>
          <SafeHtml className="legal-document-preview" html={savedBody} />
        </div>
      )}
      {pickerElement}
    </div>
  );
}
