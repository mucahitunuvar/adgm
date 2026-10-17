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
 * `enabled` (varsayılan `true`): `GET /public/submission-tokens` da diğer tüm anonim-yazma uçlarıyla
 * (form/bülten/çerez onayı gönderimi) AYNI dar `public-forms` kota penceresini (varsayılan 10/dk/IP,
 * `RateLimiting:PublicFormsPermitLimit`) paylaşır. `NewsletterSection` `Footer` üzerinden HER sayfada
 * mount olur - `enabled`i çağıran, formun GERÇEKTEN gösterileceği koşulla (`site.newsletterEnabled &&
 * privacyNotice != null`) vermezse, bülten kapalıyken bile her sayfa görüntülemesi bu kotadan bir hak
 * tüketir ve gerçek form/kayıt gönderimlerini sırf sayfalar gezinildiği için reddedilir hale getirir -
 * bu kalıbın bulunma nedeni tam olarak budur (bkz. completion raporu).
 */
export function useSubmissionToken(enabled = true): string | null {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    if (!enabled) return;
    let cancelled = false;
    const client = createApiClient({ baseUrl: window.location.origin });
    createSubmissionToken(client)
      .then((response) => {
        if (!cancelled) setToken(response.token);
      })
      .catch(() => {
        // Sessizce yok sayılır: token olmadan gönderim backend'de zaten reddedilir.
      });
    return () => {
      cancelled = true;
    };
  }, [enabled]);

  return token;
}
