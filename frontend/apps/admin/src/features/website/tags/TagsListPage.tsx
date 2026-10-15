import { deleteTag, getApiErrorMessage, listTags, mergeTag, updateTag, type TagResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  ConfirmDialog,
  DataTable,
  FilterBar,
  PageHeader,
  Pagination,
  TextField,
  formatDateTime,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { EditTagDialog } from './EditTagDialog';
import { MergeTagDialog } from './MergeTagDialog';

const PAGE_SIZE = 20;

export function TagsListPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [draftSearch, setDraftSearch] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [editingTag, setEditingTag] = useState<TagResponse | null>(null);
  const [mergingTag, setMergingTag] = useState<TagResponse | null>(null);
  const [deletingTag, setDeletingTag] = useState<TagResponse | null>(null);

  const queryKey = ['website-tags', search, page] as const;
  const { data, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => listTags(client, { search: search || undefined, page, pageSize: PAGE_SIZE }),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSearch(draftSearch);
    setPage(1);
  }

  function handleReset() {
    setDraftSearch('');
    setSearch('');
    setPage(1);
  }

  function onMutated(message: string) {
    showToast(message, 'success');
    void queryClient.invalidateQueries({ queryKey: ['website-tags'] });
  }

  function onMutationError(mutationError: unknown) {
    showToast(getApiErrorMessage(mutationError), 'danger');
  }

  const updateMutation = useMutation({
    mutationFn: (input: { id: string; name: string }) => updateTag(client, input.id, { name: input.name }),
    onSuccess: () => {
      onMutated('Etiket güncellendi.');
      setEditingTag(null);
    },
    onError: onMutationError,
  });

  const mergeMutation = useMutation({
    mutationFn: (input: { sourceId: string; targetId: string }) => mergeTag(client, input.sourceId, input.targetId),
    onSuccess: () => {
      onMutated('Etiketler birleştirildi.');
      setMergingTag(null);
    },
    onError: onMutationError,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteTag(client, id),
    onSuccess: () => {
      onMutated('Etiket silindi.');
      setDeletingTag(null);
    },
    onError: onMutationError,
  });

  const columns: DataTableColumn<TagResponse>[] = [
    { key: 'name', header: 'Ad', render: (row) => row.name },
    { key: 'slug', header: 'Slug', render: (row) => row.slug },
    { key: 'languageCode', header: 'Dil', render: (row) => row.languageCode },
    { key: 'usageCount', header: 'Kullanım', render: (row) => row.usageCount, align: 'end' },
    { key: 'createdAtUtc', header: 'Oluşturulma', render: (row) => formatDateTime(row.createdAtUtc) },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Etiketler" description="İçeriklere atanan etiketler. Birleştirme işlemi geri alınamaz." />
      <FilterBar onSubmit={handleSubmit} onReset={handleReset}>
        <TextField id="tags-filter-search" label="Ara" value={draftSearch} onChange={(event) => setDraftSearch(event.target.value)} />
      </FilterBar>
      <DataTable
        caption="Etiketler"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <>
            <button type="button" className="button button--quiet" onClick={() => setEditingTag(row)}>
              Düzenle
            </button>{' '}
            <button type="button" className="button button--quiet" onClick={() => setMergingTag(row)}>
              Birleştir
            </button>{' '}
            <button type="button" className="button button--quiet" onClick={() => setDeletingTag(row)}>
              Sil
            </button>
          </>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}

      <EditTagDialog
        key={editingTag?.id ?? 'none'}
        tag={editingTag}
        isSubmitting={updateMutation.isPending}
        onConfirm={(name) => editingTag && updateMutation.mutate({ id: editingTag.id, name })}
        onCancel={() => setEditingTag(null)}
      />
      <MergeTagDialog
        key={mergingTag?.id ?? 'none'}
        sourceTag={mergingTag}
        isSubmitting={mergeMutation.isPending}
        onConfirm={(targetId) => mergingTag && mergeMutation.mutate({ sourceId: mergingTag.id, targetId })}
        onCancel={() => setMergingTag(null)}
      />
      <ConfirmDialog
        open={Boolean(deletingTag)}
        title="Etiketi sil"
        description={`"${deletingTag?.name}" etiketini silmek istediğinize emin misiniz? ${
          deletingTag && Number(deletingTag.usageCount) > 0
            ? `Bu etiket ${deletingTag.usageCount} içerikte kullanılıyor; bağlar kopacak.`
            : ''
        }`}
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deletingTag && deleteMutation.mutate(deletingTag.id)}
        onCancel={() => setDeletingTag(null)}
      />
    </div>
  );
}
