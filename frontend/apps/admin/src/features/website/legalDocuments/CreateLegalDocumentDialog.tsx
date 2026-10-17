import { LegalDocumentKind, type CreateLegalDocumentRequest } from '@gm/api-client';
import { AccessibleDialog, Button, SelectField, TextField } from '@gm/ui';
import { useId, useState, type FormEvent } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';

const kindOptions = enumOptions(LegalDocumentKind, 'LegalDocumentKind');

export function CreateLegalDocumentDialog({
  open,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  open: boolean;
  isSubmitting: boolean;
  onConfirm: (request: CreateLegalDocumentRequest) => void;
  onCancel: () => void;
}) {
  const [key, setKey] = useState('');
  const [kind, setKind] = useState<string>(LegalDocumentKind.PrivacyNotice);
  const [title, setTitle] = useState('');
  const keyId = useId();
  const kindId = useId();
  const titleId = useId();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm({ key, kind, defaultLanguageTitle: title });
  }

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yeni yasal metin">
      <h2>Yeni yasal metin</h2>
      <form onSubmit={handleSubmit}>
        <TextField
          id={keyId}
          label="Anahtar"
          hint="Örn. kvkk-contact, kvkk-event, cookie-policy. Oluşturulduktan sonra değiştirilemez."
          required
          value={key}
          onChange={(event) => setKey(event.target.value)}
        />
        <SelectField id={kindId} label="Tür" required value={kind} onChange={(event) => setKind(event.target.value)}>
          {kindOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </SelectField>
        <TextField
          id={titleId}
          label="Başlık (varsayılan dil)"
          required
          value={title}
          onChange={(event) => setTitle(event.target.value)}
        />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
            Vazgeç
          </Button>
          <Button type="submit" disabled={isSubmitting || !key || !title}>
            {isSubmitting ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
