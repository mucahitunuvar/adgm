import { createSlider, getApiErrorMessage, getSliders, type SliderSummaryResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import {
  AccessibleDialog,
  Button,
  DataTable,
  EmptyState,
  ErrorState,
  LoadingState,
  PageHeader,
  TextField,
  buttonClassName,
  useToast,
  type DataTableColumn,
} from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';

export function SlidersListPage() {
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [key, setKey] = useState('');
  const [name, setName] = useState('');

  const { data, isLoading, error } = useQuery({ queryKey: ['website-sliders'], queryFn: () => getSliders(client) });

  const createMutation = useMutation({
    mutationFn: () => createSlider(client, { key: key || null, defaultLanguageName: name || null }),
    onSuccess: () => {
      showToast('Slider oluşturuldu.', 'success');
      setIsCreateOpen(false);
      setKey('');
      setName('');
      void queryClient.invalidateQueries({ queryKey: ['website-sliders'] });
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  const columns: DataTableColumn<SliderSummaryResponse>[] = [
    { key: 'key', header: 'Anahtar', render: (row) => row.key },
    { key: 'name', header: 'Ad', render: (row) => row.name },
    { key: 'slideCount', header: 'Slide sayısı', render: (row) => row.slideCount, align: 'end' },
  ];

  return (
    <div className="container section">
      <PageHeader eyebrow="Site yönetimi" title="Slider'lar" actions={<Button onClick={() => setIsCreateOpen(true)}>Yeni slider</Button>} />
      {isLoading ? (
        <LoadingState />
      ) : error ? (
        <ErrorState description={getApiErrorMessage(error)} />
      ) : (data ?? []).length === 0 ? (
        <EmptyState title="Henüz slider yok" description="Yeni slider oluşturun." />
      ) : (
        <DataTable
          caption="Slider'lar"
          columns={columns}
          rows={data ?? []}
          getRowKey={(row) => row.id}
          rowActions={(row) => (
            <Link to={ROUTES.websiteSliderDetail(row.id)} className={buttonClassName('quiet')}>
              Görüntüle
            </Link>
          )}
        />
      )}
      <AccessibleDialog open={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Yeni slider">
        <h2>Yeni slider</h2>
        {/* Backend `CreateSliderRequest.Key`/`DefaultLanguageName`'i nullable olarak tanımlasa da
            `CreateSliderCommandValidator` ikisini de `NotEmpty()` ister - OpenAPI'nin nullable işaretiyle
            çelişiyor (bkz. rapordaki backend/OpenAPI bulgusu), bu yüzden ikisi de burada zorunlu. */}
        <TextField id="new-slider-key" label="Anahtar" required value={key} onChange={(e) => setKey(e.target.value)} />
        <TextField id="new-slider-name" label="Ad (varsayılan dil)" required value={name} onChange={(e) => setName(e.target.value)} />
        <div className="confirm-dialog__actions">
          <Button type="button" tone="quiet" onClick={() => setIsCreateOpen(false)}>
            Vazgeç
          </Button>
          <Button type="button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending || !key.trim() || !name.trim()}>
            {createMutation.isPending ? 'Oluşturuluyor…' : 'Oluştur'}
          </Button>
        </div>
      </AccessibleDialog>
    </div>
  );
}
