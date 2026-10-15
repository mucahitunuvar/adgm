import type { SiteLanguageResponse } from '@gm/api-client';
import { AccessibleDialog, Button, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';

export interface EditLanguageDialogProps {
  language: SiteLanguageResponse | null;
  isSubmitting: boolean;
  onConfirm: (input: { name: string; sortOrder: number }) => void;
  onCancel: () => void;
}

/** Çağıran taraf `key={language?.id}` vermelidir: değişen hedefe göre tazelenmiş durumla yeniden kurulur. */
export function EditLanguageDialog({ language, isSubmitting, onConfirm, onCancel }: EditLanguageDialogProps) {
  const [name, setName] = useState(language?.name ?? '');
  const [sortOrder, setSortOrder] = useState(Number(language?.sortOrder ?? 0));
  const nameId = useId();
  const sortId = useId();

  if (!language) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm({ name, sortOrder });
  }

  return (
    <AccessibleDialog open={Boolean(language)} onClose={onCancel} title={`${language.code} dilini düzenle`}>
      <h2>{language.code} dilini düzenle</h2>
      <form onSubmit={handleSubmit}>
        <TextField id={nameId} label="Ad" required value={name} onChange={(e) => setName(e.target.value)} />
        <TextField id={sortId} label="Sıra" type="number" required value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} />
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
