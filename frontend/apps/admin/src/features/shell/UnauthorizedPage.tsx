import { useAuth } from '@gm/auth';
import { Button, PageHeader, buttonClassName } from '@gm/ui';
import { Link, useNavigate } from 'react-router-dom';
import { ROUTES } from '../../routes';

/**
 * `ProtectedRoute`'un rol uyuşmazlığında yönlendirdiği sayfa. Candidate/Employer rolüyle buraya
 * gelen bir kullanıcı oturum açmış durumdadır; bu yüzden kenar menü yerine tek başına, net bir
 * "yetkiniz yok" mesajı ve çıkış eylemi gösterilir (FRONTEND-FAZ1-MASTER-PROMPT.md Kapsam).
 */
export function UnauthorizedPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate(ROUTES.login, { replace: true });
  }

  return (
    <main className="container section auth-page">
      <PageHeader
        title="Bu panele yetkiniz yok"
        description={
          user
            ? `${user.email} ile giriş yaptınız, ancak bu hesabın yönetim paneline erişim yetkisi yok.`
            : 'Bu sayfayı görüntülemek için yetkili bir hesapla giriş yapmanız gerekir.'
        }
      />
      {user ? (
        <Button onClick={handleLogout}>Çıkış yap</Button>
      ) : (
        <Link to={ROUTES.login} className={buttonClassName('primary')}>
          Girişe dön
        </Link>
      )}
    </main>
  );
}
