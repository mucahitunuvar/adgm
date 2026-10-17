import { AccessibleDialog, Button, DateTimeField } from '@gm/ui';
import { useState } from 'react';

export function PublishLegalDocumentDraftDialog({
  open,
  versionNumber,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  open: boolean;
  versionNumber: number | string | null;
  isSubmitting: boolean;
  onConfirm: (effectiveAtUtc: string | null) => void;
  onCancel: () => void;
}) {
  const [effectiveAtUtc, setEffectiveAtUtc] = useState<string | null>(null);

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Taslağı yayınla">
      <h2>Taslağı yayınla</h2>
      <p>
        v{versionNumber} yürürlüğe girer; önceki yayınlı sürüm yürürlükten kaldırılır. Public formlar ve bülten/etkinlik kaydı bu sürümü
        gösterir ve kullanıcının kabul ettiği sürüm olarak saklar. Yayınlanan bir sürüm bir daha değiştirilemez.
      </p>
      <DateTimeField
        id="legal-document-publish-effective-at"
        label="Yürürlük tarihi"
        hint="Boş bırakılırsa hemen yürürlüğe girer. Gelecekteki bir tarih seçilirse, o tarihe kadar önceki sürüm geçerli kalır."
        value={effectiveAtUtc}
        onChange={setEffectiveAtUtc}
      />
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onConfirm(effectiveAtUtc)} disabled={isSubmitting}>
          {isSubmitting ? 'Yayınlanıyor…' : 'Yayınla'}
        </Button>
      </div>
    </AccessibleDialog>
  );
}
