import { getApiErrorMessage, getMediaAssetById, getMediaAssets, type MediaAssetDetailResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, TextField, useToast } from '@gm/ui';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { MediaAssetGrid } from './MediaAssetGrid';
import { UploadMediaDialog } from './UploadMediaDialog';

/**
 * Gövdede/ayarlarda görsel seçmenin TEK yolu: liste yalnız `MediaAssetSummaryResponse` (küçük
 * önizleme URL'si) döner, bu yüzden seçim anında `getMediaAssetById` ile tam çözünürlüklü
 * `originalUrl` ve çeviri (alt metin) alınır - RichTextEditor'ın `onRequestImage`'ı ve Identity
 * ayarlarının medya kimliği alanları aynı akışı paylaşır (bkz. `useMediaPicker.ts`).
 */
export function MediaPickerDialog({
  open,
  onCancel,
  onSelect,
}: {
  open: boolean;
  onCancel: () => void;
  onSelect: (asset: MediaAssetDetailResponse) => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [search, setSearch] = useState('');
  const [isUploadOpen, setIsUploadOpen] = useState(false);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['website-media-picker', search],
    queryFn: () => getMediaAssets(client, { search: search || undefined, kind: 'Image', pageSize: 48 }),
    enabled: open,
  });

  const selectMutation = useMutation({
    mutationFn: (assetId: string) => getMediaAssetById(client, assetId),
    onSuccess: onSelect,
    onError: (selectError) => showToast(getApiErrorMessage(selectError), 'danger'),
  });

  if (!open) return null;

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Medya seç" className="media-picker-dialog">
      <TextField id="media-picker-search" label="Ara" value={search} onChange={(event) => setSearch(event.target.value)} />
      <Button type="button" tone="secondary" onClick={() => setIsUploadOpen(true)}>
        Yeni yükle
      </Button>
      <MediaAssetGrid
        items={data?.items ?? []}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        actionLabel="Seç"
        onAction={(asset) => selectMutation.mutate(asset.id)}
      />
      <UploadMediaDialog
        open={isUploadOpen}
        onCancel={() => setIsUploadOpen(false)}
        onUploaded={(uploaded) => {
          setIsUploadOpen(false);
          void refetch();
          selectMutation.mutate(uploaded.id);
        }}
      />
    </AccessibleDialog>
  );
}
