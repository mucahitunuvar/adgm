import { resendVerificationEmail } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Brand, Button, useToast } from '@gm/ui';
import {
  BarChart3,
  ClipboardList,
  Cookie,
  FileText,
  Globe,
  Image,
  Inbox,
  LayoutDashboard,
  LayoutPanelTop,
  LayoutTemplate,
  ListTree,
  Mail,
  MessageSquareWarning,
  ScrollText,
  Scale,
  Settings,
  SignpostBig,
  SlidersHorizontal,
  Tag,
  User,
  Users,
} from 'lucide-react';
import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { ROUTES } from './routes';

interface MenuItem {
  to: string;
  label: string;
  icon: typeof LayoutDashboard;
}

/** Oturum gereken tüm sayfaların kabuğu: üst bar, rol tabanlı kenar menü, e-posta doğrulama bandı. */
export function AdminShell() {
  const { user, client, logout } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [isResending, setIsResending] = useState(false);

  async function handleLogout() {
    await logout();
    navigate(ROUTES.login);
  }

  async function handleResendVerification() {
    if (!user) return;
    setIsResending(true);
    try {
      await resendVerificationEmail(client, user.email);
      showToast('Doğrulama e-postası yeniden gönderildi.', 'success');
    } catch {
      showToast('Doğrulama e-postası gönderilemedi. Lütfen daha sonra tekrar deneyin.', 'danger');
    } finally {
      setIsResending(false);
    }
  }

  const menuItems: MenuItem[] = [
    { to: ROUTES.dashboard, label: 'Panel', icon: LayoutDashboard },
    ...(user?.role === 'Admin'
      ? [
          { to: ROUTES.users, label: 'Kullanıcılar', icon: Users },
          { to: ROUTES.auditLog, label: 'Denetim kaydı', icon: ScrollText },
          { to: ROUTES.websiteSettings, label: 'Site ayarları', icon: Settings },
          { to: ROUTES.websiteLanguages, label: 'Diller', icon: Globe },
          { to: ROUTES.websiteContentTypes, label: 'İçerik tipleri', icon: LayoutTemplate },
          { to: ROUTES.websiteContentItems, label: 'İçerikler', icon: FileText },
          { to: ROUTES.websiteMedia, label: 'Medya kütüphanesi', icon: Image },
          { to: ROUTES.websiteTags, label: 'Etiketler', icon: Tag },
          { to: ROUTES.websiteMenus, label: 'Menüler', icon: ListTree },
          { to: ROUTES.websiteSliders, label: "Slider'lar", icon: SlidersHorizontal },
          { to: ROUTES.websiteImpactMetrics, label: 'Rakamlar şeridi', icon: BarChart3 },
          { to: ROUTES.websitePopups, label: 'Pop-up / Banner', icon: MessageSquareWarning },
          { to: ROUTES.websiteHomeLayout, label: 'Ana sayfa düzeni', icon: LayoutPanelTop },
          { to: ROUTES.websiteLegalDocuments, label: 'Yasal metinler', icon: Scale },
          { to: ROUTES.websiteNewsletterSubscribers, label: 'Bülten aboneleri', icon: Mail },
          { to: ROUTES.websiteCookieConsentSummary, label: 'Çerez özeti', icon: Cookie },
          { to: ROUTES.websiteRedirects, label: 'Yönlendirmeler', icon: SignpostBig },
          { to: ROUTES.websiteForms, label: 'Formlar', icon: ClipboardList },
          { to: ROUTES.websiteFormSubmissions, label: 'Form başvuruları', icon: Inbox },
        ]
      : []),
    { to: ROUTES.account, label: 'Hesabım', icon: User },
  ];

  return (
    <div className="app-panel admin-shell">
      <a className="skip-link" href="#admin-main">
        İçeriğe geç
      </a>
      <header className="admin-topbar">
        <Brand compact />
        <div className="admin-user">
          <span>
            {user ? `${user.email} · ${user.role === 'CareerAdvisor' ? 'Danışman' : 'Yönetici'}` : 'Oturum açılmadı'}
          </span>
          <Button tone="quiet" onClick={handleLogout}>
            Çıkış yap
          </Button>
        </div>
      </header>
      <div className="admin-body">
        <nav className="admin-sidebar" aria-label="Yönetim menüsü">
          {menuItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end
              className={({ isActive }) => `admin-sidebar__link${isActive ? ' admin-sidebar__link--active' : ''}`}
            >
              <item.icon aria-hidden="true" />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <main id="admin-main" className="admin-content">
          {user && !user.emailConfirmed && (
            <p className="status status--warning admin-email-banner" role="status">
              <span>E-posta adresiniz henüz doğrulanmadı.</span>
              <Button tone="quiet" onClick={handleResendVerification} disabled={isResending}>
                {isResending ? 'Gönderiliyor…' : 'Doğrulama e-postasını yeniden gönder'}
              </Button>
            </p>
          )}
          <Outlet />
        </main>
      </div>
    </div>
  );
}
