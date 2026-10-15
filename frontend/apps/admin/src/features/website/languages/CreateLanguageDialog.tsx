import { AccessibleDialog, Button, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';

export interface CreateLanguageDialogProps {
  open: boolean;
  isSubmitting: boolean;
  onConfirm: (input: { code: string; name: string; sortOrder: number }) => void;
  onCancel: () => void;
}

/** Dil kodu (ör. "tr", "en") oluşturulduktan sonra değiştirilemez; backend `UpdateSiteLanguageRequest`'te kabul etmiyor. */
export function CreateLanguageDialog({ open, isSubmitting, onConfirm, onCancel }: CreateLanguageDialogProps) {
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [sortOrder, setSortOrder] = useState(0);
  const codeId = useId();
  const nameId = useId();
  const sortId = useId();

  if (!open) return null;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm({ code, name, sortOrder });
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yeni dil ekle">
      <h2>Yeni dil ekle</h2>
      <form onSubmit={handleSubmit}>
        <TextField id={codeId} label="Dil kodu" hint="ör. tr, en — oluşturduktan sonra değiştirilemez." required value={code} onChange={(e) => setCode(e.target.value)} />
        <TextField id={nameId} label="Ad" required value={name} onChange={(e) => setName(e.target.value)} />
        <TextField id={sortId} label="Sıra" type="number" required value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting || !code || !name}>
            {isSubmitting ? 'Kaydediliyor…' : 'Oluştur'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
