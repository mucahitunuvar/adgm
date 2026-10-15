import {
  compareContentItemRevisions,
  getApiErrorMessage,
  getContentItemRevisions,
  restoreContentItemRevision,
  type ContentItemRevisionSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, Pagination, SelectField, formatDateTime, useToast, type DataTableColumn } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getEnumLabel } from '../../../i18n/getEnumLabel';

const PAGE_SIZE = 10;

export function ContentItemRevisionsPanel({
  contentItemId,
  defaultLanguageCode,
  onRestored,
}: {
  contentItemId: string;
  defaultLanguageCode: string;
  onRestored: () => void;
}) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [compareFrom, setCompareFrom] = useState<number | null>(null);

  const queryKey = ['website-content-item-revisions', contentItemId, page] as const;
  const { data, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getContentItemRevisions(client, contentItemId, { page, pageSize: PAGE_SIZE }),
  });

  const restoreMutation = useMutation({
    mutationFn: (revisionNumber: number) =>
      restoreContentItemRevision(client, contentItemId, revisionNumber, { rowVersion: null, languageCode: null, restoreCategories: true }),
    onSuccess: () => {
      showToast('Revizyon geri yüklendi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-content-item', contentItemId] });
      void queryClient.invalidateQueries({ queryKey });
      onRestored();
    },
    onError: (restoreError) => showToast(getApiErrorMessage(restoreError), 'danger'),
  });

  const columns: DataTableColumn<ContentItemRevisionSummaryResponse>[] = [
    { key: 'revisionNumber', header: '#', render: (row) => row.revisionNumber },
    { key: 'savedAtUtc', render: (row) => formatDateTime(row.savedAtUtc), header: 'Tarih' },
    { key: 'kind', header: 'Tür', render: (row) => getEnumLabel('ContentItemRevisionKind', row.kind) },
    { key: 'changedLanguages', header: 'Değişen diller', render: (row) => row.changedLanguages.join(', ') },
    { key: 'isPublishedSnapshot', header: 'Yayın anlık görüntüsü', render: (row) => (row.isPublishedSnapshot ? 'Evet' : 'Hayır') },
  ];

  return (
    <div>
      <DataTable
        caption="Revizyonlar"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => String(row.revisionNumber)}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <>
            <Button type="button" tone="quiet" onClick={() => setCompareFrom(Number(row.revisionNumber))}>
              Karşılaştırmak için seç
            </Button>{' '}
            <Button type="button" tone="quiet" onClick={() => restoreMutation.mutate(Number(row.revisionNumber))} disabled={restoreMutation.isPending}>
              Bu revizyonu geri yükle
            </Button>
          </>
        )}
      />
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}
      {compareFrom !== null && (
        <RevisionCompareView
          contentItemId={contentItemId}
          from={compareFrom}
          defaultLanguageCode={defaultLanguageCode}
          onClose={() => setCompareFrom(null)}
        />
      )}
    </div>
  );
}

function RevisionCompareView({
  contentItemId,
  from,
  defaultLanguageCode,
  onClose,
}: {
  contentItemId: string;
  from: number;
  defaultLanguageCode: string;
  onClose: () => void;
}) {
  const { client } = useAuth();
  const [to, setTo] = useState<'current' | number>('current');

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-item-revision-compare', contentItemId, from, to, defaultLanguageCode],
    queryFn: () => compareContentItemRevisions(client, contentItemId, { from, to, lang: defaultLanguageCode }),
  });

  const fields: Array<{ key: 'title' | 'summary' | 'body' | 'metaTitle' | 'metaDescription' | 'canonicalUrl'; label: string }> = [
    { key: 'title', label: 'Başlık' },
    { key: 'summary', label: 'Özet' },
    { key: 'body', label: 'Gövde' },
    { key: 'metaTitle', label: 'Meta başlık' },
    { key: 'metaDescription', label: 'Meta açıklama' },
    { key: 'canonicalUrl', label: 'Canonical URL' },
  ];

  return (
    <div className="revision-compare">
      <h3>
        Revizyon #{from} → <SelectField id="revision-compare-to" label="Karşılaştırılan" value={String(to)} onChange={(e) => setTo(e.target.value === 'current' ? 'current' : Number(e.target.value))}>
          <option value="current">Şu an</option>
        </SelectField>
      </h3>
      {isLoading && <p>Yükleniyor…</p>}
      {error && <p className="field-error">{getApiErrorMessage(error)}</p>}
      {data && (
        <table className="revision-diff-table">
          <thead>
            <tr>
              <th>Alan</th>
              <th>Önce</th>
              <th>Sonra</th>
            </tr>
          </thead>
          <tbody>
            {fields.map(({ key, label }) => {
              const diff = data[key] as { changed: boolean; from: string | null; to: string | null };
              return (
                <tr key={key} className={diff.changed ? 'revision-diff-table__row--changed' : undefined}>
                  <td>{label}</td>
                  <td>{diff.from}</td>
                  <td>{diff.to}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
      <Button type="button" tone="quiet" onClick={onClose}>
        Kapat
      </Button>
    </div>
  );
}
