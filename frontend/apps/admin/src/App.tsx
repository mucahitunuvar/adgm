import { AuthProvider, ProtectedRoute } from '@gm/auth';
import { ToastProvider } from '@gm/ui';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AdminShell } from './AdminShell';
import { AccountPage } from './features/account/AccountPage';
import { AuditLogPage } from './features/auditLog/AuditLogPage';
import { ForgotPasswordPage } from './features/auth/ForgotPasswordPage';
import { LoginPage } from './features/auth/LoginPage';
import { ResetPasswordPage } from './features/auth/ResetPasswordPage';
import { VerifyEmailPage } from './features/auth/VerifyEmailPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { NotFoundPage } from './features/shell/NotFoundPage';
import { UnauthorizedPage } from './features/shell/UnauthorizedPage';
import { UserDetailPage } from './features/users/UserDetailPage';
import { UsersListPage } from './features/users/UsersListPage';
import { ContentTypeDetailPage } from './features/website/contentTypes/ContentTypeDetailPage';
import { ContentTypesListPage } from './features/website/contentTypes/ContentTypesListPage';
import { LanguagesPage } from './features/website/languages/LanguagesPage';
import { SettingsPage } from './features/website/settings/SettingsPage';
import { TagsListPage } from './features/website/tags/TagsListPage';
import { ROUTES } from './routes';

const queryClient = new QueryClient();

export function App() {
  return (
    // Tarayıcıdan her zaman kendi origin'ine (`/api/...`) gidilir: geliştirmede Vite'ın
    // server.proxy'si, üretimde IIS aynı origin'e yönlendirir (FRONTEND-MIMARI.md §5, K-03).
    // Backend origin'ine (VITE_BACKEND_ORIGIN) doğrudan gitmek CORS'a çarpar - backend CORS
    // yapılandırmaz ("CORS kullanılmaz (aynı origin)").
    <AuthProvider baseUrl={window.location.origin}>
      <ToastProvider>
        <QueryClientProvider client={queryClient}>
          <BrowserRouter basename="/admin">
            <Routes>
              <Route path={ROUTES.login} element={<LoginPage />} />
              <Route path={ROUTES.forgotPassword} element={<ForgotPasswordPage />} />
              <Route path={ROUTES.resetPassword} element={<ResetPasswordPage />} />
              <Route path={ROUTES.verifyEmail} element={<VerifyEmailPage />} />
              <Route path={ROUTES.unauthorized} element={<UnauthorizedPage />} />

              <Route element={<ProtectedRoute roles={['Admin', 'CareerAdvisor']} />}>
                <Route element={<AdminShell />}>
                  <Route path={ROUTES.dashboard} element={<DashboardPage />} />
                  <Route path={ROUTES.account} element={<AccountPage />} />
                  <Route element={<ProtectedRoute roles={['Admin']} unauthorizedTo={ROUTES.dashboard} />}>
                    <Route path={ROUTES.users} element={<UsersListPage />} />
                    <Route path="/kullanicilar/:userId" element={<UserDetailPage />} />
                    <Route path={ROUTES.auditLog} element={<AuditLogPage />} />
                    <Route path={ROUTES.websiteSettings} element={<SettingsPage />} />
                    <Route path={ROUTES.websiteLanguages} element={<LanguagesPage />} />
                    <Route path={ROUTES.websiteContentTypes} element={<ContentTypesListPage />} />
                    <Route path="/site-yonetimi/icerik-tipleri/:contentTypeId" element={<ContentTypeDetailPage />} />
                    <Route path={ROUTES.websiteTags} element={<TagsListPage />} />
                  </Route>
                </Route>
              </Route>

              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </BrowserRouter>
        </QueryClientProvider>
      </ToastProvider>
    </AuthProvider>
  );
}
