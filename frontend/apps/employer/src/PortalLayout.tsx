import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { Brand, Button } from '@gm/ui';
import { useAuth } from '@gm/auth';

/** Demodan taşınan kabuk (components/layout/PortalLayout.tsx), gerçek oturuma uyarlandı. */
export function PortalLayout({ children }: { children: ReactNode }) {
  const { user, logout, isLoading } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/');
  }

  return (
    <div className="app-panel portal-shell">
      <a className="skip-link" href="#portal-main">İçeriğe geç</a>
      <header className="portal-topbar">
        <Brand compact />
        <div className="portal-user">
          <span>{isLoading ? 'Oturum kontrol ediliyor…' : user ? `${user.email} · ${user.role}` : 'Oturum açılmadı'}</span>
          {user && <Button tone="quiet" onClick={handleLogout}>Çıkış yap</Button>}
        </div>
      </header>
      <main id="portal-main" className="portal-content">
        {children}
      </main>
    </div>
  );
}
