import { ENUM_NUMERIC_CODES, type EnumName } from './enums.generated';

/**
 * Sayı gelen enum alanları için (ör. `EventRegistration*Response.Status`), manifestteki
 * sayısal değerden üye adına döner. Eşleşme yoksa `null` (asla fırlatmaz).
 */
export function fromNumeric<TName extends EnumName>(
  enumName: TName,
  value: number,
): keyof (typeof ENUM_NUMERIC_CODES)[TName] | null {
  const codes: Record<string, number> = ENUM_NUMERIC_CODES[enumName];
  for (const member of Object.keys(codes)) {
    if (codes[member] === value) return member as keyof (typeof ENUM_NUMERIC_CODES)[TName];
  }
  return null;
}
