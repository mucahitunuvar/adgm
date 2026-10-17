import { LegalDocumentVersionStatus, type LegalDocumentVersionResponse } from '@gm/api-client';
import { DataTable, EmptyState, StatusBadge, formatDateTime, type DataTableColumn, type StatusTone } from '@gm/ui';
import { getEnumLabel } from '../../../i18n/getEnumLabel';

const STATUS_TONE: Record<string, StatusTone> = {
  [LegalDocumentVersionStatus.Draft]: 'info',
  [LegalDocumentVersionStatus.Published]: 'success',
  [LegalDocumentVersionStatus.Superseded]: 'neutral',
};

/** Salt okunur: yayınlanan bir sürüm hiçbir zaman düzenlenemez (ADR-024 §12.1). */
export function LegalDocumentVersionHistory({ versions }: { versions: readonly LegalDocumentVersionResponse[] }) {
  if (versions.length === 0) {
    return <EmptyState title="Henüz sürüm yok" description="İlk taslağı oluşturup yayınlayın." />;
  }

  const columns: DataTableColumn<LegalDocumentVersionResponse>[] = [
    { key: 'versionNumber', header: 'Sürüm', render: (row) => `v${row.versionNumber}` },
    {
      key: 'status',
      header: 'Durum',
      render: (row) => <StatusBadge tone={STATUS_TONE[row.status] ?? 'neutral'}>{getEnumLabel('LegalDocumentVersionStatus', row.status)}</StatusBadge>,
    },
    { key: 'effectiveAtUtc', header: 'Yürürlük tarihi', render: (row) => (row.effectiveAtUtc ? formatDateTime(row.effectiveAtUtc) : '—') },
    { key: 'publishedAtUtc', header: 'Yayın tarihi', render: (row) => (row.publishedAtUtc ? formatDateTime(row.publishedAtUtc) : '—') },
    { key: 'changeSummary', header: 'Değişiklik özeti', render: (row) => row.changeSummary ?? '—' },
  ];

  return (
    <DataTable
      caption="Sürüm geçmişi"
      columns={columns}
      rows={[...versions].sort((a, b) => Number(b.versionNumber) - Number(a.versionNumber))}
      getRowKey={(row) => row.id}
    />
  );
}
