import { RedirectStatusCode, RedirectTargetKind, type NotFoundLogResponse } from '@gm/api-client';
import { AccessibleDialog, Button } from '@gm/ui';
import { useState } from 'react';
import { RedirectTargetFields, validateRedirectTargetPath, type RedirectTargetValue } from '../shared/RedirectTargetFields';

const EMPTY_TARGET: RedirectTargetValue = {
  targetKind: RedirectTargetKind.Path,
  targetContentItemId: null,
  targetPath: null,
  statusCode: RedirectStatusCode.MovedPermanently,
};

export function ConvertNotFoundPathDialog({
  open,
  path,
  isSubmitting,
  onConfirm,
  onCancel,
}: {
  open: boolean;
  path: NotFoundLogResponse | null;
  isSubmitting: boolean;
  onConfirm: (target: RedirectTargetValue) => void;
  onCancel: () => void;
}) {
  const [target, setTarget] = useState<RedirectTargetValue>(EMPTY_TARGET);

  if (!path) return null;

  const targetPathError = target.targetKind === RedirectTargetKind.Path && target.targetPath ? validateRedirectTargetPath(target.targetPath) : null;
  const isValid = target.targetKind === RedirectTargetKind.ContentItem ? Boolean(target.targetContentItemId) : Boolean(target.targetPath) && !targetPathError;

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Yönlendirmeye çevir">
      <h2>Yönlendirmeye çevir</h2>
      <p>
        <strong>/{path.path}</strong> ({path.hitCount} isabet) için bir yönlendirme oluşturulacak; bu 404 kaydı tek bir işlemde silinir.
      </p>
      <RedirectTargetFields idPrefix="convert-404" value={target} onChange={setTarget} />
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onConfirm(target)} disabled={isSubmitting || !isValid}>
          {isSubmitting ? 'Oluşturuluyor…' : 'Yönlendirmeye çevir'}
        </Button>
      </div>
    </AccessibleDialog>
  );
}
