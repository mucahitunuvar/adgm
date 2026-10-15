import { getApiErrorMessage, getMediaAssets } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FilterBar, PageHeader, Pagination, SelectField, TextField, useToast } from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { MediaAssetGrid } from './MediaAssetGrid';
import { UploadMediaDialog } from './UploadMediaDialog';

export function MediaLibraryPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [kind, setKind] = useState('');
  const [page, setPage] = useState(1);
  const [isUploadOpen, setIsUploadOpen] = useState(false);
  const pageSize = 24;

  const queryKey = ['website-media', { search, kind, page }] as const;
  const { data, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getMediaAssets(client, { search: search || undefined, kind: kind || undefined, page, pageSize }),
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Medya kütüphanesi"
        description="İçerik ve ayarlarda kullanılacak görseller yalnız buradan yüklenir."
        actions={<Button onClick={() => setIsUploadOpen(true)}>Medya yükle</Button>}
      />
      <FilterBar
        onSubmit={(event) => {
          event.preventDefault();
          setPage(1);
        }}
        onReset={() => {
          setSearch('');
          setKind('');
          setPage(1);
        }}
      >
        <TextField id="media-search" label="Ara" value={search} onChange={(event) => setSearch(event.target.value)} />
        <SelectField id="media-kind" label="Tür" value={kind} onChange={(event) => setKind(event.target.value)}>
          <option value="">Tümü</option>
          <option value="Image">Görsel</option>
          <option value="Document">Belge</option>
        </SelectField>
      </FilterBar>
      <MediaAssetGrid
        items={data?.items ?? []}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        actionLabel="Görüntüle"
        onAction={() => showToast('Medya ayrıntı ekranı sonraki dilimde eklenecek.', 'info')}
      />
      {data && <Pagination page={page} pageSize={pageSize} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
      <UploadMediaDialog
        open={isUploadOpen}
        onCancel={() => setIsUploadOpen(false)}
        onUploaded={() => {
          setIsUploadOpen(false);
          void queryClient.invalidateQueries({ queryKey: ['website-media'] });
        }}
      />
    </div>
  );
}
