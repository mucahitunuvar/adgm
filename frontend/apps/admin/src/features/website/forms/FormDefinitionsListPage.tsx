import { getApiErrorMessage, getFormDefinitions, type FormDefinitionSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, PageHeader, StatusBadge, buttonClassName, type DataTableColumn } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { CreateFormDefinitionDialog } from './CreateFormDefinitionDialog';

export function FormDefinitionsListPage() {
  const { client } = useAuth();
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-forms'],
    queryFn: () => getFormDefinitions(client),
  });

  const columns: DataTableColumn<FormDefinitionSummaryResponse>[] = [
    { key: 'key', header: 'Anahtar', render: (row) => row.key },
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    { key: 'fieldCount', header: 'Alan sayısı', render: (row) => row.fieldCount, align: 'end' },
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
        title="Formlar"
        description="Her form; alanlarını, aydınlatma metnini ve açık rıza metinlerini tanımlar. Bir içeriğe bağlanarak public'te gösterilir."
        actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni form</Button>}
      />
      <DataTable
        caption="Formlar"
        columns={columns}
        rows={data ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        emptyTitle="Form bulunamadı"
        emptyDescription="Henüz hiç form tanımlanmamış."
        rowActions={(row) => (
          <Link to={ROUTES.websiteFormDetail(row.id)} className={buttonClassName('quiet')}>
            Görüntüle
          </Link>
        )}
      />
      <CreateFormDefinitionDialog open={isCreateOpen} onCancel={() => setIsCreateOpen(false)} />
    </div>
  );
}
