import { getApiErrorMessage, updateLegalDocumentTranslation } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, TextField, UnsavedChangesGuard, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';

/** Çağıran taraf `key={languageCode}` vermelidir: dil değişince tazelenmiş durumla yeniden kurulur. */
export function LegalDocumentTitlePanel({
  documentId,
  languageCode,
  rowVersion,
  initialTitle,
  onSaved,
}: {
  documentId: string;
  languageCode: string;
  rowVersion: string;
  initialTitle: string;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [title, setTitle] = useState(initialTitle);
  const isDirty = title !== initialTitle;

  const saveMutation = useMutation({
    mutationFn: () => updateLegalDocumentTranslation(client, documentId, languageCode, { rowVersion, title }),
    onSuccess: () => {
      showToast('Başlık kaydedildi.', 'success');
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
      <TextField
        id={`legal-document-title-${languageCode}`}
        label="Başlık"
        required
        value={title}
        onChange={(event) => setTitle(event.target.value)}
      />
      <Button type="button" tone="quiet" onClick={() => setTitle(initialTitle)} disabled={!isDirty || saveMutation.isPending}>
        Değişiklikleri geri al
      </Button>{' '}
      <Button type="submit" disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Başlığı kaydet'}
      </Button>
    </form>
  );
}
