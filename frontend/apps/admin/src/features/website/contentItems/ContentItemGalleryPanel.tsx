import {
  getApiErrorMessage,
  setContentItemGallery,
  type ContentItemDetailResponse,
  type ContentItemGalleryItemResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, TextField, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useMediaPicker } from '../../media/useMediaPicker';

const MAX_GALLERY_ITEMS = 100;

/**
 * Her görselin çeviri listesi (`translations`) tüm aktif dillerin alt/caption override'larını
 * taşır; bu panel düzenlemeyi yalnız içeriğin varsayılan diline indirir (karmaşıklığı sınırlamak
 * için) ama DİĞER dillerin override'larını kaydederken asla kaybetmez - mevcut öğenin diğer dil
 * girdileri değişmeden korunur, yalnız varsayılan dilinki güncellenir/eklenir.
 */
export function ContentItemGalleryPanel({
  contentItemId,
  detail,
  defaultLanguageCode,
  onSaved,
}: {
  contentItemId: string;
  detail: ContentItemDetailResponse;
  defaultLanguageCode: string;
  onSaved: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const { pickMedia, pickerElement } = useMediaPicker();
  const [items, setItems] = useState<ContentItemGalleryItemResponse[]>(
    [...detail.galleryItems].sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder)),
  );
  const isDirty = JSON.stringify(items) !== JSON.stringify([...detail.galleryItems].sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder)));

  const saveMutation = useMutation({
    mutationFn: () =>
      setContentItemGallery(client, contentItemId, {
        rowVersion: detail.rowVersion,
        items: items.map((item, index) => ({
          mediaAssetId: item.mediaAssetId,
          sortOrder: index,
          translations: item.translations.map((t) => ({ languageCode: t.languageCode, altTextOverride: t.altTextOverride, captionOverride: t.captionOverride })),
        })),
      }),
    onSuccess: () => {
      showToast('Galeri güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  async function handleAdd() {
    const asset = await pickMedia();
    if (!asset || items.some((i) => i.mediaAssetId === asset.id)) return;
    setItems([...items, { mediaAssetId: asset.id, sortOrder: items.length, translations: [] }]);
  }

  function handleRemove(mediaAssetId: string) {
    setItems(items.filter((i) => i.mediaAssetId !== mediaAssetId));
  }

  function handleMove(index: number, direction: -1 | 1) {
    const target = index + direction;
    if (target < 0 || target >= items.length) return;
    const next = [...items];
    [next[index], next[target]] = [next[target]!, next[index]!];
    setItems(next);
  }

  function getDefaultOverride(item: ContentItemGalleryItemResponse) {
    return item.translations.find((t) => t.languageCode === defaultLanguageCode);
  }

  function setDefaultOverride(mediaAssetId: string, field: 'altTextOverride' | 'captionOverride', value: string) {
    setItems(
      items.map((item) => {
        if (item.mediaAssetId !== mediaAssetId) return item;
        const others = item.translations.filter((t) => t.languageCode !== defaultLanguageCode);
        const current = getDefaultOverride(item) ?? { languageCode: defaultLanguageCode, altTextOverride: null, captionOverride: null };
        return { ...item, translations: [...others, { ...current, [field]: value || null }] };
      }),
    );
  }

  return (
    <div>
      <ul className="field-list">
        {items.map((item, index) => {
          const override = getDefaultOverride(item);
          return (
            <li key={item.mediaAssetId} className="field-row">
              <span>{item.mediaAssetId}</span>
              <TextField
                id={`gallery-alt-${item.mediaAssetId}`}
                label="Alt metin (varsayılan dil)"
                value={override?.altTextOverride ?? ''}
                onChange={(event) => setDefaultOverride(item.mediaAssetId, 'altTextOverride', event.target.value)}
              />
              <TextField
                id={`gallery-caption-${item.mediaAssetId}`}
                label="Altyazı (varsayılan dil)"
                value={override?.captionOverride ?? ''}
                onChange={(event) => setDefaultOverride(item.mediaAssetId, 'captionOverride', event.target.value)}
              />
              <Button type="button" tone="quiet" onClick={() => handleMove(index, -1)} disabled={index === 0}>
                Yukarı
              </Button>
              <Button type="button" tone="quiet" onClick={() => handleMove(index, 1)} disabled={index === items.length - 1}>
                Aşağı
              </Button>
              <Button type="button" tone="quiet" onClick={() => handleRemove(item.mediaAssetId)}>
                Kaldır
              </Button>
            </li>
          );
        })}
      </ul>
      <Button type="button" tone="secondary" onClick={() => void handleAdd()} disabled={items.length >= MAX_GALLERY_ITEMS}>
        Görsel ekle
      </Button>{' '}
      <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
      {pickerElement}
    </div>
  );
}
