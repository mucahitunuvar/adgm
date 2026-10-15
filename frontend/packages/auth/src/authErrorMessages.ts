import { ApiError } from '@gm/api-client';

/**
 * Backend `Error` kodları (title) → kullanıcıya gösterilecek Türkçe mesaj. Kaynak: ilgili
 * CommandHandler'lardaki `Error.*(...)` çağrıları (src/Modules/Identity/Features/**).
 * `Auth.InvalidCredentials` kasıtlı olarak hem "hesap yok" hem "parola yanlış" için aynı genel
 * mesajı taşır (anti-enumeration, FRONTEND-GUVENLIK.md §6).
 */
const TITLE_MESSAGES: Readonly<Record<string, string>> = {
  'Auth.InvalidCredentials': 'E-posta veya parola hatalı.',
  'Auth.AccountLocked':
    'Çok sayıda başarısız giriş denemesi nedeniyle hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.',
  'Auth.AccountDeactivated': 'Bu hesap devre dışı bırakılmış.',
  'Auth.AccountNotActive': 'Bu hesap şu anda aktif değil.',
  'Auth.InvalidRefreshToken': 'Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.',
  'Auth.RefreshTokenReuseDetected': 'Güvenlik nedeniyle tüm oturumlarınız sonlandırıldı. Lütfen tekrar giriş yapın.',
  'Auth.InvalidResetToken': 'Parola sıfırlama bağlantısının süresi dolmuş veya geçersiz.',
  'Auth.InvalidVerificationToken': 'E-posta doğrulama bağlantısının süresi dolmuş veya geçersiz.',
  'Auth.NotAuthenticated': 'Oturumunuz bulunamadı. Lütfen tekrar giriş yapın.',
  'User.NotFound': 'Kullanıcı bulunamadı.',
  'User.EmailAlreadyExists': 'Bu e-posta adresi zaten kayıtlı.',
};

/**
 * Herhangi bir auth/kullanıcı-yönetimi hatasını gösterilebilir Türkçe metne çevirir.
 * 401/403/429 middleware kaynaklıysa (boş gövde) `ApiError.fromResponse` zaten durum koduna göre
 * güvenli bir varsayılan `title` üretir (bkz. packages/api-client/src/ApiError.ts); burada yalnız
 * 429 için `Retry-After` süresi ayrıca eklenir.
 */
export function getAuthErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Beklenmeyen bir hata oluştu.';
  }

  if (error.status === 429) {
    return error.retryAfterSeconds
      ? `Çok sık denediniz. Lütfen ${error.retryAfterSeconds} saniye sonra tekrar deneyin.`
      : 'Çok sık denediniz. Lütfen bir süre sonra tekrar deneyin.';
  }

  return TITLE_MESSAGES[error.title] ?? error.detail ?? error.title;
}
