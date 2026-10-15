import type { MediaAssetSummaryResponse } from '@gm/api-client';
import { Button, EmptyState, ErrorState, LoadingState, StatusBadge } from '@gm/ui';

/** Medya kütüphanesi ve `MediaPickerDialog` tarafından paylaşılan sunumsal kart ızgarası. */
export function MediaAssetGrid({
  items,
  isLoading,
  error,
  actionLabel,
  onAction,
}: {
  items: MediaAssetSummaryResponse[];
  isLoading: boolean;
  error?: string;
  actionLabel: string;
  onAction: (asset: MediaAssetSummaryResponse) => void;
}) {
  if (isLoading) return <LoadingState />;
  if (error) return <ErrorState description={error} />;
  if (items.length === 0) return <EmptyState title="Medya bulunamadı" description="Bu filtrelerle eşleşen bir varlık yok." />;

  return (
    <ul className="media-asset-grid">
      {items.map((asset) => (
        <li key={asset.id} className="media-asset-grid__item">
          <img src={asset.thumbnailUrl} alt="" width={asset.width ?? undefined} height={asset.height ?? undefined} />
          <p className="media-asset-grid__filename">{asset.originalFileName}</p>
          {!asset.hasAltText && (
            <StatusBadge tone="warning">Alt metin eksik</StatusBadge>
          )}
          <Button type="button" tone="secondary" onClick={() => onAction(asset)}>
            {actionLabel}
          </Button>
        </li>
      ))}
    </ul>
  );
}
