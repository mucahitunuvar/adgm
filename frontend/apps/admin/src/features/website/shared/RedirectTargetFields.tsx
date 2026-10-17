import { RedirectStatusCode, RedirectTargetKind } from '@gm/api-client';
import { Button, SelectField, TextField } from '@gm/ui';
import { enumOptions } from '../../../i18n/enumOptions';
import { useContentItemPicker } from '../contentItems/useContentItemPicker';

const statusCodeOptions = enumOptions(RedirectStatusCode, 'RedirectStatusCode');

export interface RedirectTargetValue {
  targetKind: string;
  targetContentItemId: string | null;
  targetPath: string | null;
  statusCode: string;
}

/**
 * UX amaçlı doğrulama - asıl kural backend'de (`Redirect.NormalizeTargetPath`): göreli hedef
 * başlıkta/sonunda `/` taşımaz (backend zaten kırpar), `\`, başta `//`, herhangi bir `:`, boşluk,
 * kontrol karakteri veya `..` segmenti içeremez; mutlak hedef yalnız `https://` olabilir.
 */
export function validateRedirectTargetPath(targetPath: string): string | null {
  const trimmed = targetPath.trim();
  if (trimmed.length === 0) return 'Hedef yol zorunludur.';
  if (/^https:\/\//i.test(trimmed)) return null;
  if (/^[a-zA-Z][a-zA-Z0-9+.-]*:/.test(trimmed)) return 'Mutlak bir hedef yalnız https:// ile başlayabilir.';
  if (trimmed.includes('\\') || trimmed.startsWith('//') || /\s/.test(trimmed) || trimmed.split('/').includes('..')) {
    return 'Geçersiz yol - "\\", başta "//", boşluk veya ".." içeremez.';
  }
  return null;
}

export function RedirectTargetFields({
  idPrefix,
  value,
  onChange,
}: {
  idPrefix: string;
  value: RedirectTargetValue;
  onChange: (value: RedirectTargetValue) => void;
}) {
  const { pickContentItem, pickerElement } = useContentItemPicker();

  async function handlePickContentItem() {
    const item = await pickContentItem();
    if (item) onChange({ ...value, targetContentItemId: item.id });
  }

  const pathError = value.targetKind === RedirectTargetKind.Path && value.targetPath ? validateRedirectTargetPath(value.targetPath) : null;

  return (
    <div className="field-row">
      <SelectField
        id={`${idPrefix}-target-kind`}
        label="Hedef türü"
        value={value.targetKind}
        onChange={(event) => onChange({ ...value, targetKind: event.target.value, targetContentItemId: null, targetPath: null })}
      >
        <option value={RedirectTargetKind.ContentItem}>İçerik</option>
        <option value={RedirectTargetKind.Path}>Yol / dış bağlantı</option>
      </SelectField>

      {value.targetKind === RedirectTargetKind.ContentItem && (
        <>
          <Button type="button" tone="quiet" onClick={() => void handlePickContentItem()}>
            {value.targetContentItemId ? 'İçeriği değiştir' : 'İçerik seç'}
          </Button>
          {pickerElement}
        </>
      )}

      {value.targetKind === RedirectTargetKind.Path && (
        <TextField
          id={`${idPrefix}-target-path`}
          label="Hedef yol"
          hint='Örn. "haberler/eski-sayfa" (başında "/" olmadan) veya mutlak bir "https://" bağlantısı.'
          error={pathError ?? undefined}
          value={value.targetPath ?? ''}
          onChange={(event) => onChange({ ...value, targetPath: event.target.value })}
        />
      )}

      <SelectField
        id={`${idPrefix}-status-code`}
        label="Yönlendirme türü"
        value={value.statusCode}
        onChange={(event) => onChange({ ...value, statusCode: event.target.value })}
      >
        {statusCodeOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </SelectField>
    </div>
  );
}
