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

  return error.detail ?? error.title;
}
