import { Button } from '@gm/ui';
import { useState } from 'react';
import { useMediaPicker } from './useMediaPicker';

/**
 * Bir medya kimliği (UUID) alanı için `MediaPickerDialog` kullanan etiketli seçim alanı -
 * Identity ayarlarındaki logo/favicon/varsayılan paylaşım görseli alanları için (Dilim 2a-2:
 * daha önce elle UUID yapıştırılan yer tutucu alanlar buraya taşındı).
 */
export function MediaPickerField({
  label,
  value,
  previewUrl,
  onChange,
}: {
  label: string;
  value: string;
  previewUrl?: string | null;
  onChange: (mediaAssetId: string) => void;
}) {
  const { pickMedia, pickerElement } = useMediaPicker();
  const [localPreviewUrl, setLocalPreviewUrl] = useState<string | null | undefined>(previewUrl);

  async function handlePick() {
    const asset = await pickMedia();
    if (!asset) return;
    onChange(asset.id);
    setLocalPreviewUrl(asset.originalUrl);
  }

  function handleClear() {
    onChange('');
    setLocalPreviewUrl(null);
  }

  return (
    <div className="media-picker-field">
      <p className="media-picker-field__label">{label}</p>
      {localPreviewUrl ? (
        <img src={localPreviewUrl} alt="" className="media-picker-field__preview" />
      ) : (
        <p className="media-picker-field__empty">Henüz ayarlanmamış.</p>
      )}
      <Button type="button" tone="secondary" onClick={() => void handlePick()}>
        {value ? 'Değiştir' : 'Seç'}
      </Button>
      {value && (
        <Button type="button" tone="quiet" onClick={handleClear}>
          Kaldır
        </Button>
      )}
      {pickerElement}
    </div>
  );
}
