/**
 * ADR-024 §12.3: gizli alan, insan kullanıcıya görünmez/erişilmez, bot doldurursa gönderim reddedilir.
 * `display:none` yerine ekran-dışı konumlama kullanılır (bazı botlar `display:none` alanları atlar).
 */
export function HoneypotField() {
  return (
    <div className="honeypot-field" aria-hidden="true">
      <label htmlFor="website">Web siteniz</label>
      <input id="website" name="website" type="text" tabIndex={-1} autoComplete="off" />
    </div>
  );
}
