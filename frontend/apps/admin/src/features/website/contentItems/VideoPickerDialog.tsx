import { getApiErrorMessage, getVideos, type VideoSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { AccessibleDialog, Button, DataTable, TextField, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';

export function VideoPickerDialog({
  open,
  onCancel,
  onSelect,
}: {
  open: boolean;
  onCancel: () => void;
  onSelect: (video: VideoSummaryResponse) => void;
}) {
  const { client } = useAuth();
  const [search, setSearch] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-video-picker', search],
    queryFn: () => getVideos(client, { search: search || undefined, isActive: true, pageSize: 48 }),
    enabled: open,
  });

  if (!open) return null;

  const columns: DataTableColumn<VideoSummaryResponse>[] = [{ key: 'title', header: 'Başlık', render: (row) => row.title }];

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Video seç">
      <TextField id="video-picker-search" label="Ara" value={search} onChange={(event) => setSearch(event.target.value)} />
      <DataTable
        caption="Videolar"
        columns={columns}
        rows={data?.items ?? []}
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
