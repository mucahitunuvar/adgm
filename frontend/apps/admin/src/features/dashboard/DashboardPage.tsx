import { useCurrentUser } from '@gm/auth';
import { PageHeader, StatusBadge, buttonClassName, formatDateTime } from '@gm/ui';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../routes';

/** Backend'den gerçekten alınabilen bir özet uç yok; sahte sayı göstermek yerine oturum bilgisi gösterilir. */
export function DashboardPage() {
  const user = useCurrentUser();

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Panel"
        title={user ? `Hoş geldiniz, ${user.email}` : 'Panel'}
        description="Yönetim paneline giriş yaptınız."
      />
      {user && (
        <dl className="dashboard-summary">
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
            <dt>Kayıt tarihi</dt>
            <dd>{formatDateTime(user.createdAtUtc)}</dd>
          </div>
        </dl>
      )}
      {user?.role === 'Admin' && (
        <div className="dashboard-links">
          <Link to={ROUTES.users} className={buttonClassName('secondary')}>
            Kullanıcıları yönet
          </Link>
          <Link to={ROUTES.auditLog} className={buttonClassName('secondary')}>
            Denetim kaydını görüntüle
          </Link>
        </div>
      )}
    </div>
  );
}
