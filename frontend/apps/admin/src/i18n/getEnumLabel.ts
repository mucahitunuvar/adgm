import { trEnumLabels } from './enum-labels.tr';

export type SupportedLanguage = 'tr';

const dictionariesByLanguage: Record<SupportedLanguage, typeof trEnumLabels> = {
  tr: trEnumLabels,
};

/**
 * Enum üye adını çeviriye çevirir. Arayüzde ham enum adı ASLA gösterilmez. Dil eklerken
 * yalnız `dictionariesByLanguage`'a yeni bir giriş eklenir; çağıran kod değişmez.
 */
export function getEnumLabel<TEnumName extends keyof typeof trEnumLabels>(
  enumName: TEnumName,
  member: string,
  lang: SupportedLanguage = 'tr',
): string {
  const dictionary = dictionariesByLanguage[lang][enumName] as Record<string, string>;
  return dictionary[member] ?? `Bilinmeyen (${member})`;
}
