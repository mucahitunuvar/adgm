import { FormFieldAllowedFileType, FormFieldType } from '@gm/api-client';
import {
  AccessibleDialog,
  Button,
  CheckboxField,
  DateTimeField,
  SelectField,
  TextField,
  TranslationTabs,
  type TranslationLanguage,
} from '@gm/ui';
import { useState } from 'react';
import { enumOptions } from '../../../i18n/enumOptions';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { newDraftFieldOption, type DraftFormField, type DraftFormFieldOption } from './formFieldTypes';

const fieldTypeOptions = enumOptions(FormFieldType, 'FormFieldType');
const allowedFileTypeOptions = enumOptions(FormFieldAllowedFileType, 'FormFieldAllowedFileType');

const CHOICE_TYPES: Array<FormFieldType> = [FormFieldType.Select, FormFieldType.MultiSelect];

// Backend invariant'ı (FormField.cs: MinFileSizeMb/MaxFileSizeMb).
const MIN_FILE_SIZE_MB = 1;
const MAX_FILE_SIZE_MB = 10;

export function FormFieldDialog({
  open,
  field,
  existingKeys,
  languages,
  onSave,
  onCancel,
}: {
  open: boolean;
  field: DraftFormField | null;
  /** Aynı formdaki diğer alanların `key`'leri - yinelenen anahtar istemcide de engellenir (asıl kontrol backend'de). */
  existingKeys: readonly string[];
  languages: readonly TranslationLanguage[];
  onSave: (next: DraftFormField) => void;
  onCancel: () => void;
}) {
  const [draft, setDraft] = useState<DraftFormField | null>(field);
  const [activeLanguage, setActiveLanguage] = useState(languages[0]?.code ?? '');

  if (!open || !draft) return null;

  function translationFor(languageCode: string) {
    return (
      draft!.translations.find((t) => t.languageCode === languageCode) ??
      { languageCode, label: '', placeholder: null, helpText: null }
    );
  }

  function setTranslation(languageCode: string, patch: Partial<DraftFormField['translations'][number]>) {
    const existing = draft!.translations.filter((t) => t.languageCode !== languageCode);
    setDraft({ ...draft!, translations: [...existing, { ...translationFor(languageCode), ...patch }] });
  }

  function updateOption(next: DraftFormFieldOption) {
    setDraft({ ...draft!, options: draft!.options.map((o) => (o.tempKey === next.tempKey ? next : o)) });
  }

  function optionLabelFor(option: DraftFormFieldOption, languageCode: string): string {
    return option.translations.find((t) => t.languageCode === languageCode)?.label ?? '';
  }

  function setOptionLabel(option: DraftFormFieldOption, languageCode: string, label: string) {
    const existing = option.translations.filter((t) => t.languageCode !== languageCode);
    updateOption({ ...option, translations: [...existing, { languageCode, label }] });
  }

  const keyTrimmed = draft.key.trim();
  // Backend'in kendi doğrulaması (`SetFormFieldsCommandValidator`, "Field key must match
  // '[a-z0-9_]' and be at most 50 characters") ile birebir - UI, backend'in reddedeceği bir anahtarı
  // kaydetmeye hiç izin vermez.
  const isKeyFormatValid = keyTrimmed === '' || (/^[a-z0-9_]+$/.test(keyTrimmed) && keyTrimmed.length <= 50);
  const isDuplicateKey = keyTrimmed !== '' && existingKeys.some((k) => k === keyTrimmed && k !== field?.key);
  const isChoiceType = CHOICE_TYPES.includes(draft.type as FormFieldType);
  const isFileType = draft.type === FormFieldType.File;
  const isTextType = draft.type === FormFieldType.Text || draft.type === FormFieldType.Textarea;
  const isDateType = draft.type === FormFieldType.Date;

  const canSave =
    keyTrimmed !== '' &&
    isKeyFormatValid &&
    !isDuplicateKey &&
    draft.type !== '' &&
    draft.translations.some((t) => t.label.trim()) &&
    // Backend invariant'ı: Select/MultiSelect alanları en az 2, en fazla 50 seçeneğe sahip olmalı.
    (!isChoiceType || (draft.options.length >= 2 && draft.options.length <= 50 && draft.options.every((option) => option.key.trim() !== ''))) &&
    // Backend invariant'ı: Dosya alanları en az bir izinli tür ve 1-10 MB aralığında bir boyut sınırı gerektirir.
    (!isFileType ||
      (draft.allowedFileTypes.length > 0 && draft.maxSizeMb !== null && draft.maxSizeMb >= MIN_FILE_SIZE_MB && draft.maxSizeMb <= MAX_FILE_SIZE_MB));

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Form alanı">
      <h2>Form alanı</h2>
      <TextField
        id="form-field-key"
        label="Alan anahtarı"
        hint="Küçük harf/sayı/alt çizgi (ör. ad_soyad); başvuru verisinde bu anahtarla saklanır, sonradan değiştirilmesi eski başvuruların eşlemesini bozabilir."
        required
        value={draft.key}
        onChange={(event) => setDraft({ ...draft, key: event.target.value })}
        error={
          isDuplicateKey
            ? 'Bu anahtar aynı formda başka bir alanda kullanılıyor.'
            : !isKeyFormatValid
              ? 'Yalnız küçük harf, sayı ve alt çizgi; en fazla 50 karakter.'
              : undefined
        }
      />
      <SelectField
        id="form-field-type"
        label="Alan türü"
        required
        value={draft.type}
        onChange={(event) => setDraft({ ...draft, type: event.target.value as FormFieldType, options: [] })}
      >
        <option value="">Seçin…</option>
        {fieldTypeOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </SelectField>
      <CheckboxField
        id="form-field-required"
        label="Zorunlu"
        checked={draft.isRequired}
        onChange={(event) => setDraft({ ...draft, isRequired: event.target.checked })}
      />

      {isTextType && (
        <div className="field-row">
          <TextField
            id="form-field-min-length"
            label="En az karakter"
            type="number"
            min={0}
            value={draft.minLength ?? ''}
            onChange={(event) => setDraft({ ...draft, minLength: event.target.value === '' ? null : Number(event.target.value) })}
          />
          <TextField
            id="form-field-max-length"
            label="En fazla karakter"
            type="number"
            min={0}
            value={draft.maxLength ?? ''}
            onChange={(event) => setDraft({ ...draft, maxLength: event.target.value === '' ? null : Number(event.target.value) })}
          />
        </div>
      )}

      {isDateType && (
        <div className="field-row">
          <DateTimeField id="form-field-date-min" label="En erken tarih" value={draft.dateMin} onChange={(value) => setDraft({ ...draft, dateMin: value })} />
          <DateTimeField id="form-field-date-max" label="En geç tarih" value={draft.dateMax} onChange={(value) => setDraft({ ...draft, dateMax: value })} />
        </div>
      )}

      {isFileType && (
        <div>
          <p className="field-hint">İzinli dosya türleri</p>
          <div className="field-row">
            {allowedFileTypeOptions.map((option) => (
              <CheckboxField
                key={option.value}
                id={`form-field-allowed-type-${option.value}`}
                label={option.label}
                checked={draft.allowedFileTypes.includes(option.value)}
                onChange={(event) =>
                  setDraft({
                    ...draft,
                    allowedFileTypes: event.target.checked
                      ? [...draft.allowedFileTypes, option.value]
                      : draft.allowedFileTypes.filter((t) => t !== option.value),
                  })
                }
              />
            ))}
          </div>
          <TextField
            id="form-field-max-size-mb"
            label="En fazla dosya boyutu (MB)"
            hint={`${MIN_FILE_SIZE_MB}-${MAX_FILE_SIZE_MB} MB arası, zorunlu.`}
            required
            type="number"
            min={MIN_FILE_SIZE_MB}
            max={MAX_FILE_SIZE_MB}
            value={draft.maxSizeMb ?? ''}
            onChange={(event) => setDraft({ ...draft, maxSizeMb: event.target.value === '' ? null : Number(event.target.value) })}
          />
        </div>
      )}

      {isChoiceType && (
        <div>
          <h3>Seçenekler</h3>
          <p className="field-hint">En az 2, en fazla 50 seçenek olmalı.</p>
          {draft.options.length === 0 && <p className="field-hint">Henüz seçenek yok.</p>}
          {draft.options.map((option) => (
            <div key={option.tempKey} className="field-row" style={{ alignItems: 'flex-start' }}>
              <TextField
                id={`form-field-option-key-${option.tempKey}`}
                label="Seçenek anahtarı"
                required
                value={option.key}
                onChange={(event) => updateOption({ ...option, key: event.target.value })}
              />
              {languages.map((language) => (
                <TextField
                  key={language.code}
                  id={`form-field-option-label-${option.tempKey}-${language.code}`}
                  label={`Etiket (${language.code})`}
                  required
                  value={optionLabelFor(option, language.code)}
                  onChange={(event) => setOptionLabel(option, language.code, event.target.value)}
                />
              ))}
              <Button
                type="button"
                tone="danger"
                onClick={() => setDraft({ ...draft, options: draft.options.filter((o) => o.tempKey !== option.tempKey) })}
              >
                Seçeneği kaldır
              </Button>
            </div>
          ))}
          <Button type="button" tone="secondary" onClick={() => setDraft({ ...draft, options: [...draft.options, newDraftFieldOption()] })}>
            Seçenek ekle
          </Button>
        </div>
      )}

      <TranslationTabs
        languages={languages}
        activeCode={activeLanguage}
        onChange={setActiveLanguage}
        renderPanel={(languageCode) => {
          const translation = translationFor(languageCode);
          return (
            <div>
              <TextField
                id={`form-field-label-${languageCode}`}
                label="Etiket"
                required
                value={translation.label}
                onChange={(event) => setTranslation(languageCode, { label: event.target.value })}
              />
              <TextField
                id={`form-field-placeholder-${languageCode}`}
                label="Yer tutucu metin"
                value={translation.placeholder ?? ''}
                onChange={(event) => setTranslation(languageCode, { placeholder: event.target.value || null })}
              />
              <TextField
                id={`form-field-help-text-${languageCode}`}
                label="Yardım metni"
                value={translation.helpText ?? ''}
                onChange={(event) => setTranslation(languageCode, { helpText: event.target.value || null })}
              />
            </div>
          );
        }}
      />

      <div className="confirm-dialog__actions">
        <Button type="button" tone="quiet" onClick={onCancel}>
          Vazgeç
        </Button>
        <Button type="button" onClick={() => onSave(draft)} disabled={!canSave}>
          Tamam
        </Button>
      </div>
    </AccessibleDialog>
  );
}

export function fieldTypeLabel(type: string): string {
  return getEnumLabel('FormFieldType', type);
}
