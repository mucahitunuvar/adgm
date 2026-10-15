import { changeUserRole, deactivateUser, getUser, reactivateUser, unlockUser, UserStatus } from '@gm/api-client';
import { getAuthErrorMessage, isUserRole, useAuth, useCurrentUser } from '@gm/auth';
import {
  Button,
  ConfirmDialog,
  ErrorState,
  LoadingState,
  PageHeader,
  StatusBadge,
  buttonClassName,
  formatDateTime,
  useToast,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getEnumLabel } from '../../i18n/getEnumLabel';
import { ROUTES } from '../../routes';
import { ChangeRoleDialog } from './ChangeRoleDialog';

type PendingAction = 'deactivate' | 'reactivate' | 'unlock' | 'role' | null;

export function UserDetailPage() {
  const { userId } = useParams<{ userId: string }>();
  const { client } = useAuth();
  const currentUser = useCurrentUser();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);

  const queryKey = ['admin-user', userId] as const;

  const { data: user, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getUser(client, userId!),
    enabled: Boolean(userId),
  });

  function onMutationSuccess(message: string) {
    showToast(message, 'success');
    setPendingAction(null);
    void queryClient.invalidateQueries({ queryKey });
  }

  function onMutationError(mutationError: unknown) {
    showToast(getAuthErrorMessage(mutationError), 'danger');
  }

  const deactivateMutation = useMutation({
    mutationFn: () => deactivateUser(client, userId!),
    onSuccess: () => onMutationSuccess('Kullanıcı pasife alındı.'),
    onError: onMutationError,
  });
  const reactivateMutation = useMutation({
    mutationFn: () => reactivateUser(client, userId!),
    onSuccess: () => onMutationSuccess('Kullanıcı yeniden etkinleştirildi.'),
    onError: onMutationError,
  });
  const unlockMutation = useMutation({
    mutationFn: () => unlockUser(client, userId!),
    onSuccess: () => onMutationSuccess('Kullanıcının kilidi açıldı.'),
    onError: onMutationError,
  });
  const changeRoleMutation = useMutation({
    mutationFn: (newRole: string) => changeUserRole(client, userId!, newRole),
    onSuccess: () => onMutationSuccess('Kullanıcının rolü güncellendi.'),
    onError: onMutationError,
  });

  if (!userId) return <ErrorState description="Kullanıcı kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !user) return <ErrorState description={error ? getAuthErrorMessage(error) : 'Kullanıcı bulunamadı.'} />;

  const isSelf = currentUser?.userId === user.userId;

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Kullanıcılar"
        title={user.email}
        actions={
          <Link to={ROUTES.users} className={buttonClassName('quiet')}>
            Listeye dön
          </Link>
        }
      />
      {isSelf && (
        <p className="status status--info" role="status">
          Bu kendi hesabınız.
        </p>
      )}
      <dl className="dashboard-summary">
        <div>
          <dt>Rol</dt>
          <dd>{getEnumLabel('UserRole', user.role)}</dd>
        </div>
        <div>
          <dt>Durum</dt>
          <dd>
            <StatusBadge tone={user.status === UserStatus.Active ? 'success' : 'neutral'}>
              {getEnumLabel('UserStatus', user.status)}
            </StatusBadge>
          </dd>
        </div>
        <div>
          <dt>E-posta doğrulaması</dt>
          <dd>
            <StatusBadge tone={user.emailConfirmed ? 'success' : 'warning'}>
              {user.emailConfirmed ? 'Doğrulandı' : 'Doğrulanmadı'}
            </StatusBadge>
          </dd>
        </div>
        <div>
          <dt>Kilit durumu</dt>
          <dd>
            {user.isLockedOut ? (
              <StatusBadge tone="danger">
                {user.lockedUntilUtc ? `Kilitli · ${formatDateTime(user.lockedUntilUtc)}'e kadar` : 'Kilitli'}
              </StatusBadge>
            ) : (
              '—'
            )}
          </dd>
        </div>
        <div>
          <dt>Başarısız giriş denemesi</dt>
          <dd>{user.failedLoginAttemptCount}</dd>
        </div>
        <div>
          <dt>Kayıt tarihi</dt>
          <dd>{formatDateTime(user.createdAtUtc)}</dd>
        </div>
      </dl>

      <div className="user-detail__actions">
        {user.status === UserStatus.Disabled ? (
          <Button onClick={() => setPendingAction('reactivate')}>Yeniden etkinleştir</Button>
        ) : (
          <Button tone="danger" onClick={() => setPendingAction('deactivate')}>
            Pasife al
          </Button>
        )}
        {user.isLockedOut && <Button onClick={() => setPendingAction('unlock')}>Kilidi aç</Button>}
        <Button tone="secondary" onClick={() => setPendingAction('role')}>
          Rolü değiştir
        </Button>
        <Link to={`${ROUTES.auditLog}?userId=${encodeURIComponent(user.userId)}`} className={buttonClassName('quiet')}>
          Denetim kaydını görüntüle
        </Link>
      </div>

      <ConfirmDialog
        open={pendingAction === 'deactivate'}
        title="Kullanıcıyı pasife al"
        description={`${user.email} adlı kullanıcıyı pasife almak istediğinize emin misiniz?`}
        isConfirming={deactivateMutation.isPending}
        onConfirm={() => deactivateMutation.mutate()}
        onCancel={() => setPendingAction(null)}
      />
      <ConfirmDialog
        open={pendingAction === 'reactivate'}
        title="Kullanıcıyı yeniden etkinleştir"
        description={`${user.email} adlı kullanıcıyı yeniden etkinleştirmek istediğinize emin misiniz?`}
        tone="primary"
        confirmLabel="Etkinleştir"
        isConfirming={reactivateMutation.isPending}
        onConfirm={() => reactivateMutation.mutate()}
        onCancel={() => setPendingAction(null)}
      />
      <ConfirmDialog
        open={pendingAction === 'unlock'}
        title="Kullanıcının kilidini aç"
        description={`${user.email} adlı kullanıcının hesap kilidini açmak istediğinize emin misiniz?`}
        tone="primary"
        confirmLabel="Kilidi aç"
        isConfirming={unlockMutation.isPending}
        onConfirm={() => unlockMutation.mutate()}
        onCancel={() => setPendingAction(null)}
      />
      {isUserRole(user.role) && (
        <ChangeRoleDialog
          open={pendingAction === 'role'}
          currentRole={user.role}
          isSubmitting={changeRoleMutation.isPending}
          onConfirm={(newRole) => changeRoleMutation.mutate(newRole)}
          onCancel={() => setPendingAction(null)}
        />
      )}
    </div>
  );
}
