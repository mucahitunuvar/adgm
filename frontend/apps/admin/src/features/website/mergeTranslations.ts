/**
 * Backend yalnız zaten var olan çevirileri döner; eksik bir aktif dil için form alanını
 * boş değerlerle başlatır (kullanıcı doldurup kaydedince o dil için çeviri oluşur).
 */
export function mergeTranslations<T extends Record<string, unknown>>(
  existing: ReadonlyArray<{ languageCode: string } & T>,
  languageCodes: readonly string[],
  emptyValue: T,
): Record<string, T> {
  const byCode = new Map(existing.map((item) => [item.languageCode, item]));
  const result: Record<string, T> = {};
  for (const code of languageCodes) {
    const found = byCode.get(code);
    result[code] = found ? { ...found } : { ...emptyValue };
  }
  return result;
}
