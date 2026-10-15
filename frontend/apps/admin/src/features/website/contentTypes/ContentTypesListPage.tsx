import { getApiErrorMessage, getContentTypes, type ContentTypeSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, PageHeader, StatusBadge, buttonClassName, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { CreateContentTypeDialog } from './CreateContentTypeDialog';

export function ContentTypesListPage() {
  const { client } = useAuth();
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-types'],
    queryFn: () => getContentTypes(client),
  });

  const columns: DataTableColumn<ContentTypeSummaryResponse>[] = [
    { key: 'key', header: 'Anahtar', render: (row) => row.key },
    { key: 'defaultLanguageName', header: 'Ad (varsayılan dil)', render: (row) => row.defaultLanguageName },
    { key: 'schemaKind', header: 'Şema', render: (row) => getEnumLabel('ContentSchemaKind', row.schemaKind) },
    { key: 'sortMode', header: 'Sıralama', render: (row) => getEnumLabel('ContentTypeSortMode', row.sortMode) },
    { key: 'contentCount', header: 'İçerik sayısı', render: (row) => row.contentCount, align: 'end' },
    {
      key: 'isActive',
      header: 'Durum',
      render: (row) => <StatusBadge tone={row.isActive ? 'success' : 'neutral'}>{row.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>,
    },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="İçerik tipleri"
        description="Her tip; içerik şeması, şablon ve desteklediği özellik kümesini (kategori, etiket, etkinlik vb.) tanımlar."
        actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni içerik tipi</Button>}
      />
      <DataTable
        caption="İçerik tipleri"
        columns={columns}
        rows={data ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <Link to={ROUTES.websiteContentTypeDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      <CreateContentTypeDialog open={isCreateOpen} onCancel={() => setIsCreateOpen(false)} />
    </div>
  );
}
