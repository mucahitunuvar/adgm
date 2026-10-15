const formatter = new Intl.DateTimeFormat('tr-TR', {
  timeZone: 'Europe/Istanbul',
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

const HAS_TIMEZONE = /[zZ]|[+-]\d{2}:\d{2}$/;

/**
 * Backend her zaman UTC döner (FRONTEND-AGENTS.md §5), ama bazı yanıtlarda ISO 8601 gövdesi
 * sondaki "Z" imlecini içermeden gelir (örn. `GetCurrentUserResponse.createdAtUtc`) — bu durumda
 * `Date` yerel saat varsayar ve yanlış sonuç üretir. Burada imleç yoksa eklenip UTC olarak
 * zorlanır, sonra Europe/Istanbul'a çevrilip tr-TR biçiminde gösterilir.
 */
export function formatDateTime(isoUtc: string): string {
  const normalized = HAS_TIMEZONE.test(isoUtc) ? isoUtc : `${isoUtc}Z`;
  return formatter.format(new Date(normalized));
}
