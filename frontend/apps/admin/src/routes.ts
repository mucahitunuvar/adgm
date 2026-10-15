/**
 * Tek kaynak rota yolları. `AdminShell`'in kenar menüsü ve `ProtectedRoute` yönlendirmeleri
 * yalnız buradan türetilir — sahte/var olmayan rota linki üretilmez (FRONTEND-FAZ1-MASTER-PROMPT.md
 * Görev 6).
 */
export const ROUTES = {
  login: '/giris',
  forgotPassword: '/sifremi-unuttum',
  resetPassword: '/sifre-sifirla',
  verifyEmail: '/eposta-dogrulama',
  unauthorized: '/yetkisiz',
  dashboard: '/',
  account: '/hesabim',
  users: '/kullanicilar',
  userDetail: (userId: string) => `/kullanicilar/${userId}`,
  auditLog: '/denetim-kaydi',
  websiteSettings: '/site-yonetimi/ayarlar',
  websiteLanguages: '/site-yonetimi/diller',
  websiteContentTypes: '/site-yonetimi/icerik-tipleri',
  websiteContentTypeDetail: (contentTypeId: string) => `/site-yonetimi/icerik-tipleri/${contentTypeId}`,
  websiteTags: '/site-yonetimi/etiketler',
  websiteMedia: '/site-yonetimi/medya',
  websiteContentItems: '/site-yonetimi/icerikler',
  websiteContentItemDetail: (contentItemId: string) => `/site-yonetimi/icerikler/${contentItemId}`,
  websiteContentTrash: '/site-yonetimi/icerikler/cop-kutusu',
} as const;
