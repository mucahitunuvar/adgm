import { ApiError } from './ApiError';

/**
 * Herhangi bir uç için genel hata metni. Modüle özgü kodlar (ör. Auth.*) için
 * `packages/auth`'taki `getAuthErrorMessage` tercih edilir; bu yalnız oradaki eşleşme
 * tablosunda olmayan uçlar (ör. Website admin) için güvenli bir varsayılandır.
 */
export function getApiErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Beklenmeyen bir hata oluştu.';
  }

  if (error.status === 429) {
    return error.retryAfterSeconds
      ? `Çok sık denediniz. Lütfen ${error.retryAfterSeconds} saniye sonra tekrar deneyin.`
      : 'Çok sık denediniz. Lütfen bir süre sonra tekrar deneyin.';
  }

  if (error.status === 409) {
    return 'Bu kayıt sizden sonra başka biri tarafından değiştirilmiş. Sayfayı yeniden yükleyip tekrar deneyin.';
  }

  // KOŞU 3G Görev 2: `PublicSubmissionGuard` her reddi (süresi dolmuş/sahte gönderim jetonu, honeypot,
  // Turnstile) aynı ayrıntısız koda indirir (AGENTS.md §27 - hangi kontrolün başarısız olduğu client'a
  // sızdırılmaz), bu yüzden burada da tek, nedene göre ayırmayan ama en olası/düzeltilebilir nedeni
  // (sayfanın `GetSubmissionTokenQueryHandler.TokenLifetime` - 2 saat - süresini aşacak kadar açık
  // kalması) işaret eden Türkçe bir mesaj gösterilir; ham İngilizce backend `detail`'i hiç basılmaz.
  if (error.title === 'PublicSubmission.Rejected') {
    return 'Gönderiminiz işlenemedi. Sayfa uzun süre açık kaldıysa yenileyip tekrar deneyin.';
  }

  return error.detail ?? error.title;
}
