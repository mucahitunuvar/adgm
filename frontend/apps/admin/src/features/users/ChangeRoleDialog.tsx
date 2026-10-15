import { UserRole as UserRoleCodes } from '@gm/api-client';
import type { UserRole } from '@gm/auth';
import { AccessibleDialog, Button, SelectField } from '@gm/ui';
import { useId, useState } from 'react';
import { enumOptions } from '../../i18n/enumOptions';

const roleOptions = enumOptions(UserRoleCodes, 'UserRole');

export interface ChangeRoleDialogProps {
  open: boolean;
  currentRole: UserRole;
  isSubmitting: boolean;
  onConfirm: (newRole: string) => void;
  onCancel: () => void;
}

export function ChangeRoleDialog({ open, currentRole, isSubmitting, onConfirm, onCancel }: ChangeRoleDialogProps) {
  const [role, setRole] = useState<UserRole>(currentRole);
  const fieldId = useId();

  if (!open) return null;

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Rolü değiştir">
      <h2>Rolü değiştir</h2>
      <SelectField id={fieldId} label="Yeni rol" value={role} onChange={(event) => setRole(event.target.value as UserRole)}>
        {roleOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </SelectField>
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel} disabled={isSubmitting}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onConfirm(role)} disabled={isSubmitting || role === currentRole}>
          {isSubmitting ? 'Kaydediliyor…' : 'Rolü güncelle'}
        </Button>
      </div>
    </AccessibleDialog>
  );
}
