import { AccessibleDialog } from './AccessibleDialog';
import { Button, type ButtonTone } from './Button';

export interface ConfirmDialogProps {
  open: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  tone?: ButtonTone;
  isConfirming?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

/** Yıkıcı/kritik eylemler için onay penceresi (`AccessibleDialog` üzerine kurulu). */
export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = 'Onayla',
  cancelLabel = 'Vazgeç',
  tone = 'danger',
  isConfirming = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <AccessibleDialog open={open} onClose={onCancel} title={title} className="confirm-dialog">
      <h2>{title}</h2>
      <p>{description}</p>
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel} disabled={isConfirming}>
          {cancelLabel}
        </Button>
        <Button type="button" tone={tone} onClick={onConfirm} disabled={isConfirming}>
          {isConfirming ? 'İşleniyor…' : confirmLabel}
        </Button>
      </div>
    </AccessibleDialog>
  );
}
