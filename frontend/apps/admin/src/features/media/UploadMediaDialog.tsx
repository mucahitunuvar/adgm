import { getApiErrorMessage, uploadMediaAsset, type UploadMediaAssetResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, TextField, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';

export function UploadMediaDialog({
  open,
  onCancel,
  onUploaded,
}: {
  open: boolean;
  onCancel: () => void;
  onUploaded: (asset: UploadMediaAssetResponse) => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [file, setFile] = useState<File | null>(null);
  const [folder, setFolder] = useState('');
  const [altText, setAltText] = useState('');
  const [caption, setCaption] = useState('');

  const mutation = useMutation({
    mutationFn: () => {
      if (!file) throw new Error('Dosya seçilmedi.');
      return uploadMediaAsset(client, { file, folder: folder || undefined, altText: altText || undefined, caption: caption || undefined });
    },
    onSuccess: (asset) => {
      showToast('Medya yüklendi.', 'success');
      setFile(null);
      setFolder('');
      setAltText('');
      setCaption('');
      onUploaded(asset);
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  if (!open) return null;

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Medya yükle">
      <form
        onSubmit={(event) => {
          event.preventDefault();
          mutation.mutate();
        }}
      >
        <div className="form-field">
          <label htmlFor="media-upload-file">
            Dosya<span aria-hidden="true"> *</span>
          </label>
          <input
            id="media-upload-file"
            type="file"
            accept="image/*"
            required
            onChange={(event) => setFile(event.target.files?.[0] ?? null)}
          />
        </div>
        <TextField id="media-upload-folder" label="Klasör" hint="Boş bırakılırsa kök klasöre yüklenir." value={folder} onChange={(event) => setFolder(event.target.value)} />
        <TextField id="media-upload-alt" label="Alt metin" value={altText} onChange={(event) => setAltText(event.target.value)} />
        <TextField id="media-upload-caption" label="Açıklama" value={caption} onChange={(event) => setCaption(event.target.value)} />
        <Button type="button" tone="quiet" onClick={onCancel}>
          Vazgeç
        </Button>{' '}
        <Button type="submit" disabled={!file || mutation.isPending}>
          {mutation.isPending ? 'Yükleniyor…' : 'Yükle'}
        </Button>
      </form>
    </AccessibleDialog>
  );
}
