import {
  createLegalDocument,
  getApiErrorMessage,
  getLegalDocuments,
  type CreateLegalDocumentRequest,
  type LegalDocumentSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DataTable, PageHeader, StatusBadge, useToast, type DataTableColumn } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getEnumLabel } from '../../../i18n/getEnumLabel';
import { ROUTES } from '../../../routes';
import { CreateLegalDocumentDialog } from './CreateLegalDocumentDialog';

export function LegalDocumentsListPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  const { data, isLoading, error } = useQuery({ queryKey: ['website-legal-documents'], queryFn: () => getLegalDocuments(client) });

  const createMutation = useMutation({
    mutationFn: (body: CreateLegalDocumentRequest) => createLegalDocument(client, body),
    onSuccess: (response) => {
      showToast('Yasal metin oluşturuldu.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-legal-documents'] });
      setIsCreateOpen(false);
      navigate(ROUTES.websiteLegalDocumentDetail(response.id));
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const columns: DataTableColumn<LegalDocumentSummaryResponse>[] = [
    { key: 'key', header: 'Anahtar', render: (row) => row.key },
    { key: 'title', header: 'Başlık', render: (row) => row.title },
    { key: 'kind', header: 'Tür', render: (row) => getEnumLabel('LegalDocumentKind', row.kind) },
    {
      key: 'effectiveVersionNumber',
      header: 'Yürürlükteki sürüm',
      render: (row) =>
        row.effectiveVersionNumber != null ? (
          <StatusBadge tone="success">{`v${row.effectiveVersionNumber}`}</StatusBadge>
        ) : (
          <StatusBadge tone="warning">Yayınlı sürüm yok</StatusBadge>
        ),
    },
    {
      key: 'hasDraft',
      header: 'Taslak',
      render: (row) => (row.hasDraft ? <StatusBadge tone="info">Taslak var</StatusBadge> : '—'),
    },
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Yasal metinler"
        description="KVKK aydınlatma metinleri, çerez politikası, kullanım şartları. Yayınlanan bir sürüm değiştirilemez; değişiklik her zaman yeni bir sürümdür."
        actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni yasal metin</Button>}
      />
      <DataTable
        caption="Yasal metinler"
        columns={columns}
        rows={data ?? []}
        getRowKey={(row) => row.id}
        isLoading={isLoading}
        error={error ? getApiErrorMessage(error) : undefined}
        emptyTitle="Henüz yasal metin yok"
        emptyDescription="Bülten, etkinlik ve çerez ayarlarının çalışması için en az bir aydınlatma metni oluşturup yayınlayın."
        rowActions={(row) => (
          <Button type="button" tone="quiet" onClick={() => navigate(ROUTES.websiteLegalDocumentDetail(row.id))}>
            Aç
          </Button>
        )}
      />
      <CreateLegalDocumentDialog
        open={isCreateOpen}
        isSubmitting={createMutation.isPending}
        onConfirm={(request) => createMutation.mutate(request)}
        onCancel={() => setIsCreateOpen(false)}
      />
    </div>
  );
}
