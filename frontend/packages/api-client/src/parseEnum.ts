/**
 * Backend'den gelen bir dizgiyi (`enums.generated.ts`'teki) bilinen bir enum üyesine
 * eşler. Bilinmeyen/boş değerde `null` döner; asla fırlatmaz (arayüz "Bilinmeyen (ham
 * değer)" gösterip çökmemelidir).
 */
export function parseEnum<TEnum extends Record<string, string>>(
  enumObj: TEnum,
  value: string | null | undefined,
): TEnum[keyof TEnum] | null {
  if (value == null) return null;
  return Object.prototype.hasOwnProperty.call(enumObj, value) ? (value as TEnum[keyof TEnum]) : null;
}
