import { getApiErrorMessage, getContentItems, type ContentItemSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, DataTable, TextField, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';

/**
 * `useMediaPicker`'ın aynı deseni: tek seçimli diyalog, ilişkili içerik veya üst içerik seçimi
 * için panel tarafından (gerekirse) tekrar tekrar açılır.
 */
export function ContentItemPickerDialog({
  open,
  excludeId,
  onCancel,
  onSelect,
}: {
  open: boolean;
  excludeId?: string;
  onCancel: () => void;
  onSelect: (item: ContentItemSummaryResponse) => void;
}) {
  const { client } = useAuth();
  const [search, setSearch] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-item-picker', search],
    queryFn: () => getContentItems(client, { search: search || undefined, pageSize: 20 }),
    enabled: open,
  });

  if (!open) return null;

  const rows = (data?.items ?? []).filter((item) => item.id !== excludeId);
  const columns: DataTableColumn<ContentItemSummaryResponse>[] = [
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    { key: 'status', header: 'Durum', render: (row) => row.status },
  ];

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="İçerik seç">
      <TextField id="content-item-picker-search" label="Ara" value={search} onChange={(event) => setSearch(event.target.value)} />
      <DataTable
        caption="İçerikler"
        columns={columns}
        rows={rows}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <Button type="button" tone="quiet" onClick={() => onSelect(row)}>
            Seç
          </Button>
        )}
      />
    </AccessibleDialog>
  );
}
