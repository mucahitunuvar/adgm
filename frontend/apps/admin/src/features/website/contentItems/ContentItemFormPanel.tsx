import { getApiErrorMessage, getFormDefinitions, setContentItemForm, type ContentItemDetailResponse } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, FormSection, SelectField, buttonClassName, useToast } from '@gm/ui';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../../routes';

/**
 * ADR-024 §12.2: bir içerik en fazla bir forma bağlanır (ör. "İletişim" sayfası "iletişim" formuna).
 * Form listesi `getFormDefinitions` sayfalama yapmaz (yönetilebilir sayıda form beklenir) - aynı
 * ölçek varsayımı `LegalDocumentSelector`'ınkiyle aynı.
 */
export function ContentItemFormPanel({ contentItemId, detail, onSaved }: { contentItemId: string; detail: ContentItemDetailResponse; onSaved: () => void }) {
  const { client } = useAuth();
  const { showToast } = useToast();
  const [selectedFormId, setSelectedFormId] = useState<string | null>(detail.formDefinitionId);
  const [loadedFormDefinitionId, setLoadedFormDefinitionId] = useState(detail.formDefinitionId);

  if (detail.formDefinitionId !== loadedFormDefinitionId) {
    setLoadedFormDefinitionId(detail.formDefinitionId);
    setSelectedFormId(detail.formDefinitionId);
  }

  // `staleTime: 0`: bu ekran bir BAĞLAMA kararı için kullanılıyor (hangi form seçiliyor, etkin mi) -
  // örn. Formlar listesinden bu sayfaya dönüldüğünde az önce etkinleştirilmiş/oluşturulmuş bir formun
  // global 30 sn'lik varsayılan `staleTime`'dan (App.tsx) dolayı eski (önbellekten) görünmesi istenmez.
  const { data: forms, isLoading } = useQuery({
    queryKey: ['website-forms'],
    queryFn: () => getFormDefinitions(client),
    staleTime: 0,
  });

  const setFormMutation = useMutation({
    mutationFn: () => setContentItemForm(client, contentItemId, { rowVersion: detail.rowVersion, formDefinitionId: selectedFormId }),
    onSuccess: () => {
      showToast('Bağlı form güncellendi.', 'success');
      onSaved();
    },
    onError: (error) => showToast(getApiErrorMessage(error), 'danger'),
  });

  const isDirty = selectedFormId !== detail.formDefinitionId;
  const boundForm = detail.formDefinitionId ? forms?.find((f) => f.id === detail.formDefinitionId) : null;

  return (
    <FormSection title="Bağlı form" description="Bu içerik public sayfasında seçilen formu gösterir. Bağlantı kaldırılırsa sayfada form görünmez.">
      {detail.formDefinitionId && (
        <p>
          Şu an bağlı: {isLoading ? 'yükleniyor…' : boundForm ? <Link to={ROUTES.websiteFormDetail(boundForm.id)}>{boundForm.title}</Link> : detail.formDefinitionId}
        </p>
      )}
      <SelectField
        id="content-item-form-select"
        label="Form"
        value={selectedFormId ?? ''}
        onChange={(event) => setSelectedFormId(event.target.value || null)}
      >
        <option value="">Yok (bağlantıyı kaldır)</option>
        {(forms ?? []).map((form) => (
          <option key={form.id} value={form.id}>
            {form.title} ({form.key}){!form.isActive ? ' — pasif' : ''}
          </option>
        ))}
      </SelectField>
      <div className="field-row">
        <Button type="button" onClick={() => setFormMutation.mutate()} disabled={!isDirty || setFormMutation.isPending}>
          {setFormMutation.isPending ? 'Kaydediliyor…' : 'Kaydet'}
        </Button>
        {boundForm && (
          <Link to={ROUTES.websiteFormDetail(boundForm.id)} className={buttonClassName('quiet')}>
            Formu yönet
          </Link>
        )}
      </div>
    </FormSection>
  );
}
