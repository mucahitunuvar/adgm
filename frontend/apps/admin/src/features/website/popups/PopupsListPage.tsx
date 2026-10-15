import {
  createPopup,
  getApiErrorMessage,
  getPopups,
  type PopupSummaryResponse,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  AccessibleDialog,
  Button,
  DataTable,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  Pagination,
  SelectField,
  StatusBadge,
  TextField,
  buttonClassName,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';

const PAGE_SIZE = 50;

export function PopupsListPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [displayMode, setDisplayMode] = useState('Modal');
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');

  const { data, isLoading, error } = useQuery({ queryKey: ['website-popups', page], queryFn: () => getPopups(client, { page, pageSize: PAGE_SIZE }) });

  const createMutation = useMutation({
    mutationFn: () =>
      createPopup(client, {
        displayMode,
        imageMediaId: null,
        link: null,
        targeting: { kind: 'AllPages', contentItemIds: null, paths: null },
        deviceTarget: 'All',
        publishAtUtc: null,
        unpublishAtUtc: null,
        delaySeconds: displayMode === 'Modal' ? 3 : 0,
        frequency: 'EveryVisit',
        frequencyDays: null,
        dismissible: true,
        priority: 0,
        defaultLanguageTitle: title || null,
        defaultLanguageBody: body || null,
        defaultLanguageButtonLabel: null,
      }),
    onSuccess: () => {
      showToast('Pop-up oluşturuldu - diğer ayarları detay sayfasından tamamlayın.', 'success');
      setIsCreateOpen(false);
      setTitle('');
      setBody('');
      void queryClient.invalidateQueries({ queryKey: ['website-popups'] });
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const columns: DataTableColumn<PopupSummaryResponse>[] = [
    { key: 'displayMode', header: 'Tür', render: (row) => (row.displayMode === 'Modal' ? 'Modal' : 'Banner') },
    { key: 'title', header: 'Başlık', render: (row) => row.title ?? '—' },
    { key: 'priority', header: 'Öncelik', render: (row) => row.priority, align: 'end' },
    {
      key: 'isActive',
      header: 'Durum',
      render: (row) => <StatusBadge tone={row.isActive ? 'success' : 'neutral'}>{row.isActive ? 'Etkin' : 'Pasif'}</StatusBadge>,
    },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Pop-up / Banner" actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni pop-up</Button>} />
      {isLoading ? (
        <LoadingState />
      ) : error ? (
        <ErrorState description={getApiErrorMessage(error)} />
      ) : (data?.items ?? []).length === 0 ? (
        <EmptyState title="Henüz pop-up yok" description="Yeni pop-up oluşturun." />
      ) : (
        <DataTable
          caption="Pop-up/Banner"
          columns={columns}
          rows={data?.items ?? []}
          getRowKey={(row) => row.id}
          rowActions={(row) => (
            <Link to={ROUTES.websitePopupDetail(row.id)} className={buttonClassName('quiet')}>
              Görüntüle
            </Link>
          )}
        />
      )}
      {data && <Pagination page={page} pageSize={PAGE_SIZE} totalCount={Number(data.totalCount)} onPageChange={setPage} />}

      <AccessibleDialog open={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Yeni pop-up">
        <h2>Yeni pop-up</h2>
        <p className="field-hint">
          Hedefleme, zamanlama, görsel ve diğer ayarlar oluşturduktan sonra detay sayfasından düzenlenir.
        </p>
        <SelectField id="new-popup-mode" label="Tür" value={displayMode} onChange={(e) => setDisplayMode(e.target.value)}>
          <option value="Modal">Modal</option>
          <option value="Banner">Banner</option>
        </SelectField>
        {/* Backend: Modal'da başlık zorunlu (domain invariant'ı, Banner'da değil) - FluentValidation
            kuralı değil, bu yüzden OpenAPI'de de görünmüyor (bkz. rapordaki backend bulgusu). */}
        <TextField
          id="new-popup-title"
          label="Başlık (varsayılan dil)"
          required={displayMode === 'Modal'}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <TextField id="new-popup-body" label="Metin (varsayılan dil)" required value={body} onChange={(e) => setBody(e.target.value)} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={() => setIsCreateOpen(false)}>
            Vazgeç
          </Button>
          <Button
            type="button"
            onClick={() => createMutation.mutate()}
            disabled={createMutation.isPending || !body.trim() || (displayMode === 'Modal' && !title.trim())}
          >
            {createMutation.isPending ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </AccessibleDialog>
    </div>
  );
}
