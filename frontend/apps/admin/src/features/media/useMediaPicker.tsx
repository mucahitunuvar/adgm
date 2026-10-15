import type { MediaAssetDetailResponse } from '@gm/api-client';
import { useRef, useState, type ReactNode } from 'react';
import { MediaPickerDialog } from './MediaPickerDialog';

/**
 * `MediaPickerDialog`'u komut (`pickMedia()`) biçiminde açar: diyaloğu kendisi monte eder,
 * kullanıcı bir varlık seçince veya vazgeçince çözülen bir Promise döner. `RichTextEditor`'ın
 * `onRequestImage` imzası (`() => Promise<...|null>`) ve Identity ayarlarındaki medya kimliği
 * alanları aynı diyaloğu bu şekilde paylaşır.
 */
export function useMediaPicker(): { pickMedia: () => Promise<MediaAssetDetailResponse | null>; pickerElement: ReactNode } {
  const [open, setOpen] = useState(false);
  const resolverRef = useRef<((value: MediaAssetDetailResponse | null) => void) | null>(null);

  function pickMedia(): Promise<MediaAssetDetailResponse | null> {
    setOpen(true);
    return new Promise((resolve) => {
      resolverRef.current = resolve;
    });
  }

  function handleSelect(asset: MediaAssetDetailResponse) {
    setOpen(false);
    resolverRef.current?.(asset);
    resolverRef.current = null;
  }

  function handleCancel() {
    setOpen(false);
    resolverRef.current?.(null);
    resolverRef.current = null;
  }

  return {
    pickMedia,
    pickerElement: <MediaPickerDialog open={open} onCancel={handleCancel} onSelect={handleSelect} />,
  };
}
