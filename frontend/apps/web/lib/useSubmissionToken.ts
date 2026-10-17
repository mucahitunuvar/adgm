'use client';

import { useEffect, useState } from 'react';
import { createApiClient, createSubmissionToken } from '@gm/api-client';

/**
 * KOŞU 3F (form gönderimi testi sırasında bulunan hata): `PublicSubmissionGuard.MinimumFillDuration`
 * (backend, 3 sn) jetonun NE ZAMAN alındığını jetonun kendi imzalı yüküne gömer ve gönderim anıyla
 * arasındaki süreyi ölçer - "gerçek bir sayfa yüklemesinden sonra gönderildiğini" kanıtlamak için.
 * Jeton gönderim TIKLANDIĞI anda istenirse (önceki hal: `FormRenderer`/`NewsletterSection`/
 * `EventRegistrationSection`'ın üçü de `handleSubmit`/`onSubmit` içinde `createSubmissionToken`
 * çağırıyordu) aradaki süre her zaman ~0 olur - mekanizma hiçbir zaman geçilemez, gerçek kullanıcı
 * gönderimleri de dahil. Jeton bu yüzden bileşen EKRANA GELİR GELMEZ (mount) istenip saklanmalı;
 * gönderim o saklanan jetonu kullanır. Henüz gelmediyse (kullanıcı anlık olarak çok hızlı gönderirse)
 * `null` döner - backend zaten aynı 3 sn kuralıyla reddeder, ayrı bir hata ele alımı gerekmez.
 *
 * KOŞU 3G Görev 2: jeton, backend'de `GetSubmissionTokenQueryHandler.TokenLifetime` ile 2 saat
 * ömürlüdür - süresi dolmuş bir jetonla gönderim, `PublicSubmissionGuard`'ın diğer her reddiyle
 * (honeypot, Turnstile) AYNI ayrıntısız hatayı döner (AGENTS.md §27), yani backend yanıtından "jeton
 * süresi doldu" diye ayırt edilemez. Bu yüzden client kendi saatine göre PROAKTİF davranır: sayfa bu
 * kadar uzun açık kalırsa (örn. bir sekme günlerce açık unutulursa) jeton, kullanıcı hiç göndermeden
 * ARKA PLANDA, süresi dolmadan ONCA önce (`REFRESH_BUFFER_MS` payıyla) kendiliğinden tazelenir - gerçek
 * kullanıcı gönderimi pratikte asla süresi dolmuş bir jetonla karşılaşmaz. Tazeleme TAMAMEN arka plan
 * zamanlayıcısıyla tetiklendiği (gönderimle hiç eşzamanlı değil) için `MinimumFillDuration`'ı asla
 * bozmaz - yeni jetonun üzerinden gönderime kadar her zaman (en az `REFRESH_BUFFER_MS` kadar) zaten
 * 3 sn'den çok daha uzun bir süre geçmiş olur. Tazeleme isteği başarısız olursa (ağ hatası vb.) kısa
 * bir süre sonra kendiliğinden yeniden denenir - sayfa açık kaldığı sürece kendi kendini onarır.
 *
 * `enabled` (varsayılan `true`): `GET /public/submission-tokens` da diğer tüm anonim-yazma uçlarıyla
 * (form/bülten/çerez onayı gönderimi) AYNI dar `public-forms` kota penceresini (varsayılan 10/dk/IP,
 * `RateLimiting:PublicFormsPermitLimit`) paylaşır. `NewsletterSection` `Footer` üzerinden HER sayfada
 * mount olur - `enabled`i çağıran, formun GERÇEKTEN gösterileceği koşulla (`site.newsletterEnabled &&
 * privacyNotice != null`) vermezse, bülten kapalıyken bile her sayfa görüntülemesi bu kotadan bir hak
 * tüketir ve gerçek form/kayıt gönderimlerini sırf sayfalar gezinildiği için reddedilir hale getirir -
 * bu kalıbın bulunma nedeni tam olarak budur (bkz. completion raporu). Arka plan tazelemesi de aynı
 * kotayı paylaştığından yalnız `enabled` iken zamanlanır.
 */
// Backend: GetSubmissionTokenQueryHandler.TokenLifetime (src/Modules/Website/Features/GetSubmissionToken).
export const TOKEN_LIFETIME_MS = 2 * 60 * 60 * 1000;
// Süresi dolmadan bu kadar önce tazelenir - ağ gecikmesi/saat kayması için tampon.
export const REFRESH_BUFFER_MS = 10 * 60 * 1000;
const REFRESH_DELAY_MS = TOKEN_LIFETIME_MS - REFRESH_BUFFER_MS;
// Bir tazeleme denemesi başarısız olursa yeniden deneme sıklığı - `public-forms` kotasını (10/dk/IP)
// zorlamayacak kadar seyrek.
const RETRY_DELAY_MS = 30_000;

export function useSubmissionToken(enabled = true): string | null {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    if (!enabled) return;
    let cancelled = false;
    let timer: ReturnType<typeof setTimeout> | undefined;
    const client = createApiClient({ baseUrl: window.location.origin });

    function scheduleNext(delayMs: number) {
      timer = setTimeout(fetchToken, delayMs);
    }

    function fetchToken() {
      createSubmissionToken(client)
        .then((response) => {
          if (cancelled) return;
          setToken(response.token);
          scheduleNext(REFRESH_DELAY_MS);
        })
        .catch(() => {
          if (cancelled) return;
          // Sessizce yok sayılır: token olmadan gönderim backend'de zaten reddedilir. Kısa bir
          // süre sonra yeniden denenir - sayfa açık kaldığı sürece kendiliğinden iyileşir.
          scheduleNext(RETRY_DELAY_MS);
        });
    }

    fetchToken();

    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, [enabled]);

  return token;
}
