import type { SessionUser } from '@gm/auth';
import { StatusBadge, formatDateTime } from '@gm/ui';

export function ProfileSummary({ user }: { user: SessionUser }) {
  return (
    <dl className="dashboard-summary">
      <div>
        <dt>E-posta</dt>
        <dd>{user.email}</dd>
      </div>
      <div>
        <dt>Rol</dt>
        <dd>{user.role === 'CareerAdvisor' ? 'Danışman' : 'Yönetici'}</dd>
      </div>
      <div>
        <dt>Hesap durumu</dt>
        <dd>
          <StatusBadge tone={user.status === 'Active' ? 'success' : 'warning'}>{user.status}</StatusBadge>
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
        <dt>Kayıt tarihi</dt>
        <dd>{formatDateTime(user.createdAtUtc)}</dd>
      </div>
    </dl>
  );
}
