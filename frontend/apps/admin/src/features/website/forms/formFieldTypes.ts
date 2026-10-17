import type { FormFieldAllowedFileType, FormFieldInput, FormFieldResponse, FormFieldType } from '@gm/api-client';

/**
 * `SetFormFields`'ın `FormFieldInput`'u hiçbir kimlik taşımaz (tüm liste sırayla değiştirilir) -
 * `tempKey` yalnız React listesi/`ReorderList` için istemci tarafında tutulan bir anahtar, backend'e
 * gitmez (aynı desen: `slideTypes.ts`'teki `DraftSlide`).
 */
export interface DraftFormFieldOption {
  tempKey: string;
  key: string;
  translations: Array<{ languageCode: string; label: string }>;
}

export interface DraftFormField {
  tempKey: string;
  key: string;
  type: FormFieldType | '';
  isRequired: boolean;
  minLength: number | null;
  maxLength: number | null;
  options: DraftFormFieldOption[];
  dateMin: string | null;
  dateMax: string | null;
  allowedFileTypes: FormFieldAllowedFileType[];
  maxSizeMb: number | null;
  translations: Array<{ languageCode: string; label: string; placeholder: string | null; helpText: string | null }>;
}

export function draftFieldsFromResponse(fields: readonly FormFieldResponse[]): DraftFormField[] {
  return fields.map((field) => ({
    tempKey: crypto.randomUUID(),
    key: field.key,
    type: field.type as FormFieldType,
    isRequired: field.isRequired,
    minLength: toNumberOrNull(field.minLength),
    maxLength: toNumberOrNull(field.maxLength),
    options: field.options.map((option) => ({
      tempKey: crypto.randomUUID(),
      key: option.key,
      translations: option.translations.map((t) => ({ languageCode: t.languageCode, label: t.label })),
    })),
    dateMin: field.dateMin,
    dateMax: field.dateMax,
    allowedFileTypes: field.allowedFileTypes as FormFieldAllowedFileType[],
    maxSizeMb: toNumberOrNull(field.maxSizeMb),
    translations: field.translations.map((t) => ({
      languageCode: t.languageCode,
      label: t.label,
      placeholder: t.placeholder,
      helpText: t.helpText,
    })),
  }));
}

export function draftFieldsToInput(fields: readonly DraftFormField[]): FormFieldInput[] {
  return fields.map((field, index) => ({
    key: field.key,
    type: field.type || null,
    isRequired: field.isRequired,
    sortOrder: index,
    minLength: field.minLength,
    maxLength: field.maxLength,
    options: field.options.map((option) => ({
      key: option.key,
      translations: option.translations.map((t) => ({ languageCode: t.languageCode, label: t.label })),
    })),
    dateMin: field.dateMin,
    dateMax: field.dateMax,
    allowedFileTypes: field.allowedFileTypes,
    maxSizeMb: field.maxSizeMb,
    translations: field.translations.map((t) => ({
      languageCode: t.languageCode,
      label: t.label,
      placeholder: t.placeholder,
      helpText: t.helpText,
    })),
  }));
}

export function newDraftField(): DraftFormField {
  return {
    tempKey: crypto.randomUUID(),
    key: '',
    type: '',
    isRequired: false,
    minLength: null,
    maxLength: null,
    options: [],
    dateMin: null,
    dateMax: null,
    allowedFileTypes: [],
    maxSizeMb: null,
    translations: [],
  };
}

export function newDraftFieldOption(): DraftFormFieldOption {
  return { tempKey: crypto.randomUUID(), key: '', translations: [] };
}

// Wire alanları `number | string` (`Format: int32`) olarak gelir; backend `string` gönderdiğinde (ör.
// büyük sayı serileşme güvenliği) her yerde tutarlı tek bir sayıya çevrilir.
function toNumberOrNull(value: number | string | null): number | null {
  if (value === null) return null;
  return typeof value === 'number' ? value : Number(value);
}
