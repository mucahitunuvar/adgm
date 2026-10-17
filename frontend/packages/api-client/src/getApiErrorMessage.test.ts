import { describe, expect, it } from 'vitest';
import { ApiError } from './ApiError';
import { getApiErrorMessage } from './getApiErrorMessage';

describe('getApiErrorMessage', () => {
  it('returns a generic message for a non-ApiError', () => {
    expect(getApiErrorMessage(new Error('boom'))).toBe('Beklenmeyen bir hata oluştu.');
  });

  it('mentions the retry delay for a 429 with Retry-After', () => {
    const error = new ApiError(429, 'Too many requests', undefined, undefined, undefined, 20);
    expect(getApiErrorMessage(error)).toBe('Çok sık denediniz. Lütfen 20 saniye sonra tekrar deneyin.');
  });

  it('asks the user to reload on a 409 conflict', () => {
    const error = new ApiError(409, 'Conflict');
    expect(getApiErrorMessage(error)).toBe('Bu kayıt sizden sonra başka biri tarafından değiştirilmiş. Sayfayı yeniden yükleyip tekrar deneyin.');
  });

  // KOŞU 3G Görev 2: PublicSubmissionGuard'ın ayrıntısız reddi (süresi dolmuş jeton, honeypot,
  // Turnstile - hepsi aynı kod) ham İngilizce backend `detail`'i yerine anlaşılır, eyleme geçirilebilir
  // bir Türkçe mesajla gösterilir.
  it('maps PublicSubmission.Rejected to a clear Turkish message instead of the raw backend detail', () => {
    const error = new ApiError(400, 'PublicSubmission.Rejected', 'This submission could not be processed.');
    expect(getApiErrorMessage(error)).toBe('Gönderiminiz işlenemedi. Sayfa uzun süre açık kaldıysa yenileyip tekrar deneyin.');
  });

  it('falls back to detail, then title, for any other ApiError', () => {
    expect(getApiErrorMessage(new ApiError(400, 'Some.Title', 'Some detail'))).toBe('Some detail');
    expect(getApiErrorMessage(new ApiError(400, 'Some.Title'))).toBe('Some.Title');
  });
});
