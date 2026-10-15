import {
  getApiErrorMessage,
  setContentItemAttachments,
  type ContentItemAttachmentResponse,
  type ContentItemDetailResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, TextField, useToast } from '@gm/ui';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useMediaPicker } from '../../media/useMediaPicker';

const MAX_ATTACHMENTS = 30;

/** Ad override düzenlemesi `ContentItemGalleryPanel`'daki gibi varsayılan dille sınırlı; diğer dillerin override'ları korunur. */
export function ContentItemAttachmentsPanel({
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
  const [items, setItems] = useState<ContentItemAttachmentResponse[]>(
    [...detail.attachments].sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder)),
  );
  const isDirty = JSON.stringify(items) !== JSON.stringify([...detail.attachments].sort((a, b) => Number(a.sortOrder) - Number(b.sortOrder)));

  const saveMutation = useMutation({
    mutationFn: () =>
      setContentItemAttachments(client, contentItemId, {
        rowVersion: detail.rowVersion,
        items: items.map((item, index) => ({
          mediaAssetId: item.mediaAssetId,
          sortOrder: index,
          translations: item.translations.map((t) => ({ languageCode: t.languageCode, displayNameOverride: t.displayNameOverride })),
        })),
      }),
    onSuccess: () => {
      showToast('Ek dosyalar güncellendi.', 'success');
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

  function setDisplayNameOverride(mediaAssetId: string, value: string) {
    setItems(
      items.map((item) => {
        if (item.mediaAssetId !== mediaAssetId) return item;
        const others = item.translations.filter((t) => t.languageCode !== defaultLanguageCode);
        return { ...item, translations: [...others, { languageCode: defaultLanguageCode, displayNameOverride: value || null }] };
      }),
    );
  }

  return (
    <div>
      <ul className="field-list">
        {items.map((item, index) => {
          const override = item.translations.find((t) => t.languageCode === defaultLanguageCode);
          return (
            <li key={item.mediaAssetId} className="field-row">
              <span>{item.mediaAssetId}</span>
              <TextField
                id={`attachment-name-${item.mediaAssetId}`}
                label="Görünen ad (varsayılan dil)"
                value={override?.displayNameOverride ?? ''}
                onChange={(event) => setDisplayNameOverride(item.mediaAssetId, event.target.value)}
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
      <Button type="button" tone="secondary" onClick={() => void handleAdd()} disabled={items.length >= MAX_ATTACHMENTS}>
        Dosya ekle
      </Button>{' '}
      <Button type="button" onClick={() => saveMutation.mutate()} disabled={!isDirty || saveMutation.isPending}>
        {saveMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
      </Button>
      {pickerElement}
    </div>
  );
}
