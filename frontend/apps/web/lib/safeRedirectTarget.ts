import { safeUrl } from '@gm/ui';

/**
 * Backend'in rota çözümleme yanıtındaki Redirect hedefini (`location`) doğrular (açık yönlendirme
 * koruması, FRONTEND-GUVENLIK.md §3 `safeUrl` ilkesinin yönlendirme hedefine uyarlanmışı).
 *
 * Yalnız iki biçim kabul edilir:
 * - "/" ile başlayan, "//" veya "/\" ile BAŞLAMAYAN kök-göreli bir yol. `safeUrl`'ün kök-göreli
 *   dalı yalnız "//" önekini reddeder; "/\" (bazı tarayıcılarda `\` `/`'e eşdeğer sayılır ve
 *   `/\evil.com` fiilen protokolden-bağımsız `//evil.com`'a dönüşür) için kendi kontrolünü yapar.
 * - `safeUrl`'den geçen, `http(s)` şemalı mutlak bir URL (`mailto:`/`tel:` bir yönlendirme hedefi
 *   olarak anlamsızdır, bu yüzden `safeUrl`'ün izin verdiği şema kümesinden daha dar).
 *
 * Aksi halde `undefined` döner; çağıran bunu `notFound()` olarak ele alır.
 */
export function safeRedirectTarget(location: string | null | undefined): string | undefined {
  if (!location) return undefined;
  const trimmed = location.trim();
  if (trimmed === '') return undefined;

  if (trimmed.startsWith('/') && !trimmed.startsWith('//') && !trimmed.startsWith('/\\')) {
    return trimmed;
  }

  const validated = safeUrl(trimmed);
  return validated && /^https?:\/\//i.test(validated) ? validated : undefined;
}
