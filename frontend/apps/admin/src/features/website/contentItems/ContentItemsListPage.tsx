import { getApiErrorMessage, getContentItems, type ContentItemSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, PageHeader, StatusBadge, buttonClassName, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { CreateContentItemDialog } from './CreateContentItemDialog';

export function ContentItemsListPage() {
  const { client } = useAuth();
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-content-items'],
    queryFn: () => getContentItems(client, { pageSize: 50 }),
  });

  const columns: DataTableColumn<ContentItemSummaryResponse>[] = [
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    { key: 'status', header: 'Durum', render: (row) => <StatusBadge tone={row.status === 'Published' ? 'success' : 'neutral'}>{getEnumLabel('ContentItemStatus', row.status)}</StatusBadge> },
    { key: 'isFeatured', header: 'Öne çıkan', render: (row) => (row.isFeatured ? 'Evet' : 'Hayır') },
    { key: 'sortOrder', header: 'Sıra', render: (row) => row.sortOrder, align: 'end' },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="İçerikler"
        description="Galeri, video, ek dosya, yayın/zamanlama ve revizyon geçmişi sonraki dilimde eklenecek; bu ekran başlık/slug/özet/gövde ve SEO ile sınırlıdır."
        actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni içerik</Button>}
      />
      <DataTable
        caption="İçerikler"
        columns={columns}
        rows={data?.items ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        rowActions={(row) => (
          <Link to={ROUTES.websiteContentItemDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      <CreateContentItemDialog open={isCreateOpen} onCancel={() => setIsCreateOpen(false)} />
    </div>
  );
}
