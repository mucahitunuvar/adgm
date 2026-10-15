import { getEnumLabel } from './getEnumLabel';
import type { trEnumLabels } from './enum-labels.tr';

export interface EnumOption<TValue extends string> {
  value: TValue;
  label: string;
}

/**
 * Üretilmiş enum nesnesinden (`@gm/api-client`, ör. `UserStatus`) `Select`/filtre/rozet
 * seçenekleri üretir. `value` her zaman backend'in beklediği üye adıdır; `label` yalnız
 * görüntü amaçlıdır ve asla backend'e gönderilmez (`enum-labels.tr.ts` üzerinden çözülür).
 */
export function enumOptions<TEnum extends Record<string, string>, TEnumName extends keyof typeof trEnumLabels>(
  enumObj: TEnum,
  enumName: TEnumName,
  options: { only?: ReadonlyArray<TEnum[keyof TEnum]>; exclude?: ReadonlyArray<TEnum[keyof TEnum]> } = {},
): Array<EnumOption<TEnum[keyof TEnum]>> {
  const members = Object.values(enumObj) as Array<TEnum[keyof TEnum]>;
  return members
    .filter((member) => (options.only ? options.only.includes(member) : true))
    .filter((member) => !options.exclude?.includes(member))
    .map((member) => ({ value: member, label: getEnumLabel(enumName, member) }));
}
