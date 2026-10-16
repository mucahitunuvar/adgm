'use client';

// Türkiye Eylül 2016'dan beri kalıcı UTC+3'te (yaz saati uygulaması yok) - bu yüzden tek bir sabit
// ofsetle çeviri güvenli; `formatDateTime.ts`'in salt-okunur gösterimi `Intl`/`Europe/Istanbul`
// kullanıyor, burada ise GERİ dönüşüm (yerel girdi -> UTC) de gerektiği için aynı sabiti kullanan
// tek bir çift yardımcı yeterli - yeni bir zaman dilimi kütüphanesi eklemeye gerek yok.
const ISTANBUL_UTC_OFFSET_HOURS = 3;
const HAS_TIMEZONE = /[zZ]|[+-]\d{2}:\d{2}$/;

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** UTC ISO 8601 -> `<input type="datetime-local">` değeri (Europe/Istanbul yerel saati, saniyesiz). */
export function utcIsoToIstanbulLocalInputValue(isoUtc: string | null | undefined): string {
  if (!isoUtc) return '';
  const normalized = HAS_TIMEZONE.test(isoUtc) ? isoUtc : `${isoUtc}Z`;
  const utcMs = new Date(normalized).getTime();
  if (Number.isNaN(utcMs)) return '';
  const istanbul = new Date(utcMs + ISTANBUL_UTC_OFFSET_HOURS * 60 * 60 * 1000);
  return (
    `${istanbul.getUTCFullYear()}-${pad(istanbul.getUTCMonth() + 1)}-${pad(istanbul.getUTCDate())}` +
    `T${pad(istanbul.getUTCHours())}:${pad(istanbul.getUTCMinutes())}`
  );
}

/** `<input type="datetime-local">` değeri (Europe/Istanbul yerel saati) -> UTC ISO 8601. */
export function istanbulLocalInputValueToUtcIso(localValue: string): string | null {
  if (!localValue) return null;
  const withOffset = `${localValue}:00+03:00`;
  const date = new Date(withOffset);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

export interface DateTimeFieldProps {
  id: string;
  label: string;
  hint?: string;
  error?: string;
  required?: boolean;
  disabled?: boolean;
  /** UTC ISO 8601 veya `null` (boş). */
  value: string | null;
  /** UTC ISO 8601 veya `null` (temizlendiğinde) ile çağrılır - dönüşüm bu bileşenin içinde olur. */
  onChange: (utcIso: string | null) => void;
}

/** Europe/Istanbul yerel saatiyle gösterir/düzenler, backend'e her zaman UTC ISO 8601 gönderir. */
export function DateTimeField({ id, label, hint, error, required, disabled, value, onChange }: DateTimeFieldProps) {
  const descriptionId = `${id}-description`;
  return (
    <div className={`form-field${error ? ' form-field--error' : ''}`}>
      <label htmlFor={id}>
        {label}
        {required ? <span aria-hidden="true"> *</span> : <span className="optional"> (isteğe bağlı)</span>}
      </label>
      {hint && (
        <p className="field-hint" id={descriptionId}>
          {hint}
        </p>
      )}
      <input
        id={id}
        type="datetime-local"
        required={required}
        disabled={disabled}
        aria-invalid={Boolean(error)}
        aria-describedby={hint || error ? descriptionId : undefined}
        value={utcIsoToIstanbulLocalInputValue(value)}
        onChange={(event) => onChange(istanbulLocalInputValueToUtcIso(event.target.value))}
      />
      {error && (
        <p className="field-error" id={descriptionId}>
          {error}
        </p>
      )}
    </div>
  );
}
