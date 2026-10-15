import type { TagResponse } from '@gm/api-client';
import { AccessibleDialog, Button, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';

/** Çağıran taraf `key={tag?.id}` vermelidir: değişen hedefe göre tazelenmiş durumla yeniden kurulur. */
export function EditTagDialog({
  tag,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  tag: TagResponse | null;
  isSubmitting: boolean;
  onConfirm: (name: string) => void;
  onCancel: () => void;
}) {
  const [name, setName] = useState(tag?.name ?? '');
  const nameId = useId();

  if (!tag) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm(name);
  }

  return (
    <AccessibleDialog open={Boolean(tag)} onClose={onCancel} title="Etiketi düzenle">
      <h2>Etiketi düzenle</h2>
      <form onSubmit={handleSubmit}>
        <TextField id={nameId} label="Ad" hint="Slug (yol parçası) backend'de otomatik yeniden üretilir." required value={name} onChange={(e) => setName(e.target.value)} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting || !name}>
            {isSubmitting ? 'Kaydediliyor…' : 'Kaydet'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
