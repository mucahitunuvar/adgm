import { assignFormSubmission, getApiErrorMessage, listUsers, UserRole, type AdminUserListItemResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, SelectField, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';

/**
 * `/auth/admin/users` (tek atama kaynağı, FRONTEND-FAZ2B-MASTER-PROMPT.md 2b-2) yalnız `Admin` rolünü
 * kabul eder (backend bulgusu), ama `AssignFormSubmission` `Website.Submissions.Manage` iznini
 * (Danışman'a da verilebilir) ister. Bir Danışman bu diyaloğu açtığında kullanıcı listesi 403 ile
 * döner - bu durumda seçici devre dışı bırakılır, mevcut atama bilgisiyle birlikte açıklayıcı bir
 * metin gösterilir; diyalog çökmez.
 */
export function AssignFormSubmissionDialog({
  open,
  submissionId,
  rowVersion,
  currentAssignedToUserId,
  onClose,
}: {
  open: boolean;
  submissionId: string;
  rowVersion: string;
  currentAssignedToUserId: string | null;
  onClose: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [selectedUserId, setSelectedUserId] = useState<string | null>(currentAssignedToUserId);

  const { data: admins, error: adminsError, isLoading: isLoadingAdmins } = useQuery({
    queryKey: ['assignable-users', UserRole.Admin],
    queryFn: () => listUsers(client, { role: UserRole.Admin, pageSize: 200 }),
    enabled: open,
  });
  const { data: advisors, isLoading: isLoadingAdvisors } = useQuery({
    queryKey: ['assignable-users', UserRole.CareerAdvisor],
    queryFn: () => listUsers(client, { role: UserRole.CareerAdvisor, pageSize: 200 }),
    enabled: open,
  });

  const assignMutation = useMutation({
    mutationFn: () => assignFormSubmission(client, submissionId, { rowVersion, assignedToUserId: selectedUserId }),
    onSuccess: () => {
      showToast('Atama güncellendi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-form-submission', submissionId] });
      void queryClient.invalidateQueries({ queryKey: ['website-form-submissions'] });
      onClose();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  if (!open) return null;

  const isForbidden = (adminsError as { status?: number } | undefined)?.status === 403;
  const users: AdminUserListItemResponse[] = [...(admins?.items ?? []), ...(advisors?.items ?? [])];
  const isLoading = isLoadingAdmins || isLoadingAdvisors;

  return (
    <AccessibleDialog open={open} onClose={onClose} title="Başvuruyu ata">
      <h2>Başvuruyu ata</h2>
      {isForbidden ? (
        <p className="field-hint">
          Kullanıcı listesi bu hesap için görüntülenemiyor (yalnız Admin rolü kullanıcı listesini çekebilir). Atamayı değiştirmek için bir Admin
          hesabıyla açın.
        </p>
      ) : (
        <SelectField
          id="assign-form-submission-user"
          label="Atanan kullanıcı"
          hint="Yalnız Admin ve Danışman rollü kullanıcılar listelenir."
          value={selectedUserId ?? ''}
          onChange={(event) => setSelectedUserId(event.target.value || null)}
          disabled={isLoading}
        >
          <option value="">Atanmamış</option>
          {users.map((user) => (
            <option key={user.userId} value={user.userId}>
              {user.email} ({user.role === UserRole.Admin ? 'Admin' : 'Danışman'})
            </option>
          ))}
        </SelectField>
      )}
      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onClose}>
          Vazgeç
        </Button>
        <Button
          type="button"
          onClick={() => assignMutation.mutate()}
          disabled={isForbidden || assignMutation.isPending || selectedUserId === currentAssignedToUserId}
        >
          {assignMutation.isPending ? 'Kaydediliyor…' : 'Ata'}
        </Button>
      </div>
    </AccessibleDialog>
  );
}
