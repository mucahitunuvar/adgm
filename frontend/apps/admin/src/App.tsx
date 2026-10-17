import { AuthProvider, ProtectedRoute } from '@gm/auth';
import { LoadingState, ToastProvider } from '@gm/ui';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { lazy, Suspense } from 'react';
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
import { MediaLibraryPage } from './features/media/MediaLibraryPage';
import { SettingsPage } from './features/website/settings/SettingsPage';
import { TagsListPage } from './features/website/tags/TagsListPage';
import { ContentItemsListPage } from './features/website/contentItems/ContentItemsListPage';
import { ContentTrashPage } from './features/website/contentItems/ContentTrashPage';
import { MenusPage } from './features/website/menus/MenusPage';
import { SlidersListPage } from './features/website/sliders/SlidersListPage';
import { SliderDetailPage } from './features/website/sliders/SliderDetailPage';
import { ImpactMetricsPage } from './features/website/impactMetrics/ImpactMetricsPage';
import { PopupsListPage } from './features/website/popups/PopupsListPage';
import { PopupDetailPage } from './features/website/popups/PopupDetailPage';
import { LegalDocumentsListPage } from './features/website/legalDocuments/LegalDocumentsListPage';
import { NewsletterSubscribersListPage } from './features/website/newsletterSubscribers/NewsletterSubscribersListPage';
import { CookieConsentSummaryPage } from './features/website/cookieConsent/CookieConsentSummaryPage';
import { RedirectsPage } from './features/website/redirects/RedirectsPage';
import { FormDefinitionsListPage } from './features/website/forms/FormDefinitionsListPage';
import { FormDefinitionDetailPage } from './features/website/forms/FormDefinitionDetailPage';
import { FormSubmissionsListPage } from './features/website/formSubmissions/FormSubmissionsListPage';
import { FormSubmissionDetailPage } from './features/website/formSubmissions/FormSubmissionDetailPage';
import { ROUTES } from './routes';

/**
 * Dışa açık: `queryClientFocus.test.tsx` gerçek uygulamanın kullandığı aynı varsayımları (30 sn
 * staleTime, odak yeniden sorgusu kapalı) ayrı bir kopyada üretip sınar - burada kullanılan örnekle
 * tesadüfen aynı değerler olduğunu varsaymak yerine. `staleTimeMs` yalnız o testin, verinin TAZE
 * kaldığı için değil `refetchOnWindowFocus: false` sayesinde yeniden sorgu atılmadığını ayrıca
 * (staleTime'ı sıfıra indirip) kanıtlayabilmesi için var - uygulamanın kendisi hiçbir yerde override
 * vermez, her zaman 30 sn'lik varsayılanı kullanır.
 */
export function createQueryClient(options?: { staleTimeMs?: number }): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Panel ekranlarının çoğu tek bir yöneticinin art arda birkaç saniye içinde yaptığı okuma/
        // yazmalardan oluşuyor; pencere odağı değiştiğinde (başka sekme/uygulamaya geçip dönmek)
        // mevcut tüm sorguları yeniden ateşlemek gerçek bir ihtiyaçtan değil varsayılan davranıştan
        // kaynaklanıyordu - ölçülen ekranlarda (ana sayfa düzeni, içerik detayı, medya kütüphanesi)
        // 20+ istek yoktu ama "authenticated" limiti (60/dk) odak değişimlerinin birikmesiyle
        // tüketilebiliyordu. Kaydetmeden önceki taze okuma (409 kontrolü) zaten doğrudan api-client
        // çağrısı olduğu için (bu cache'i hiç kullanmıyor) bu değişiklik o akışları etkilemiyor.
        staleTime: options?.staleTimeMs ?? 30_000,
        refetchOnWindowFocus: false,
      },
    },
  });
}

const queryClient = createQueryClient();

// A11 §1: zengin metin düzenleyici (@gm/rich-text, Tiptap/ProseMirror) ağırlıklıdır - panelin ilk
// yüküne eklenmez, yalnız bu sayfalara girildiğinde ayrı bir parça (chunk) olarak indirilir.
// `HomeLayoutPage`/`ContentItemDetailPage` (ikincisi `ContentLayoutPanel` üzerinden) ikisi de
// `BlockFieldRenderer`'ın `rich-text` bloğu için kullandığı `RichTextEditor`'a ulaşıyor - ikisi de
// lazy olmazsa bu ağır bağımlılık ana pakete (eager import zinciri) sızıyordu (ölçüldü: 945 kB/282 kB
// gzip ana paket - bkz. FRONTEND-ACIK-MADDELER.md A11).
const ContentItemDetailPage = lazy(() => import('./features/website/contentItems/ContentItemDetailPage'));
const HomeLayoutPage = lazy(() => import('./features/website/layouts/HomeLayoutPage'));
// Taslak gövde düzenleyicisi (`LegalDocumentDraftBodyPanel`) de `@gm/rich-text` kullanır - yukarıdaki
// not aynen geçerli.
const LegalDocumentDetailPage = lazy(() => import('./features/website/legalDocuments/LegalDocumentDetailPage'));

/**
 * `<Suspense>`'in parça (chunk) indirilirken gösterdiği geçici hal - yalnız `LoadingState` kullanmak
 * axe'ın `page-has-heading-one` kuralını düşürüyordu: parça indirilene kadar sayfada HİÇ h1 yok
 * (gerçek bir kullanıcı için de geçerli, yalnız test bulgusu değil - yavaş bir ağda bu birkaç saniye
 * sürebilir). Her rota kendi gerçek başlığını kendi `PageHeader`'ıyla yükledikten sonra basar; bu
 * geçici an için görsel olmayan (sr-only) bir başlık yeterli.
 */
function RouteLoadingFallback() {
  return (
    <div className="container section">
      <h1 className="sr-only">Yükleniyor</h1>
      <LoadingState />
    </div>
  );
}

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
                    <Route path={ROUTES.websiteMedia} element={<MediaLibraryPage />} />
                    <Route path={ROUTES.websiteContentItems} element={<ContentItemsListPage />} />
                    <Route path={ROUTES.websiteContentTrash} element={<ContentTrashPage />} />
                    <Route path={ROUTES.websiteMenus} element={<MenusPage />} />
                    <Route path={ROUTES.websiteSliders} element={<SlidersListPage />} />
                    <Route path="/site-yonetimi/slider/:id" element={<SliderDetailPage />} />
                    <Route path={ROUTES.websiteImpactMetrics} element={<ImpactMetricsPage />} />
                    <Route path={ROUTES.websitePopups} element={<PopupsListPage />} />
                    <Route path="/site-yonetimi/popup/:id" element={<PopupDetailPage />} />
                    <Route path={ROUTES.websiteLegalDocuments} element={<LegalDocumentsListPage />} />
                    <Route
                      path="/site-yonetimi/yasal-metinler/:id"
                      element={
                        <Suspense fallback={<RouteLoadingFallback />}>
                          <LegalDocumentDetailPage />
                        </Suspense>
                      }
                    />
                    <Route path={ROUTES.websiteNewsletterSubscribers} element={<NewsletterSubscribersListPage />} />
                    <Route path={ROUTES.websiteCookieConsentSummary} element={<CookieConsentSummaryPage />} />
                    <Route path={ROUTES.websiteRedirects} element={<RedirectsPage />} />
                    <Route path={ROUTES.websiteForms} element={<FormDefinitionsListPage />} />
                    <Route path="/site-yonetimi/formlar/:formId" element={<FormDefinitionDetailPage />} />
                    <Route path={ROUTES.websiteFormSubmissions} element={<FormSubmissionsListPage />} />
                    <Route path="/site-yonetimi/form-basvurulari/:submissionId" element={<FormSubmissionDetailPage />} />
                    <Route
                      path={ROUTES.websiteHomeLayout}
                      element={
                        <Suspense fallback={<RouteLoadingFallback />}>
                          <HomeLayoutPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/site-yonetimi/icerikler/:contentItemId"
                      element={
                        <Suspense fallback={<RouteLoadingFallback />}>
                          <ContentItemDetailPage />
                        </Suspense>
                      }
                    />
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
