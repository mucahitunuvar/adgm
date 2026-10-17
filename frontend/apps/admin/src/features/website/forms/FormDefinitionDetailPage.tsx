import { activateFormDefinition, deactivateFormDefinition, deleteFormDefinition, getApiErrorMessage, getFormDefinitionById, getSiteLanguages } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, buttonClassName, ConfirmDialog, ErrorState, LoadingState, PageHeader, Tabs, type TabItem, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { FormDefinitionGeneralForm } from './FormDefinitionGeneralForm';
import { FormDefinitionTranslationForm } from './FormDefinitionTranslationForm';
import { FormFieldsPanel } from './FormFieldsPanel';

export function FormDefinitionDetailPage() {
  const { formId } = useParams<{ formId: string }>();
  const { client } = useAuth();
  const { data } = useQuery({
    queryKey: ['website-form', formId],
    queryFn: () => getFormDefinitionById(client, formId!),
    enabled: Boolean(formId),
  });

  return (
    <div className="container section">
      <PageHeader eyebrow="Formlar" title={data?.key ?? 'Form'} description={data ? `Anahtar: ${data.key}` : undefined} />
      <FormDefinitionDetailEditor />
    </div>
  );
}

// `PageHeader`'ın (sayfanın tek h1'i) üstteki sarmalayıcıda KOŞULSUZ render edilmesi gerekir - axe
// `page-has-heading-one` yükleme anında PageHeader'dan önce dönen bir `return <LoadingState />`'i
// yakalar (bkz. `SliderDetailPage`'in aynı notu).
function FormDefinitionDetailEditor() {
  const { formId } = useParams<{ formId: string }>();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('general');
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);

  const queryKey = ['website-form', formId] as const;
  const { data: detail, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getFormDefinitionById(client, formId!),
    enabled: Boolean(formId),
  });
  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });

  function onSaved() {
    void queryClient.invalidateQueries({ queryKey });
  }

  const activateMutation = useMutation({
    mutationFn: () => activateFormDefinition(client, formId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('Form etkinleştirildi.', 'success');
      onSaved();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });
  const deactivateMutation = useMutation({
    mutationFn: () => deactivateFormDefinition(client, formId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('Form pasifleştirildi.', 'success');
      onSaved();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteFormDefinition(client, formId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('Form silindi.', 'success');
      void queryClient.invalidateQueries({ queryKey: ['website-forms'] });
      navigate(ROUTES.websiteForms);
    },
    onError: (mutationError) => {
      showToast(getApiErrorMessage(mutationError), 'danger');
      setIsDeleteOpen(false);
    },
  });

  if (!formId) return <ErrorState description="Form kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !detail) return <ErrorState description={error ? getApiErrorMessage(error) : 'Form bulunamadı.'} />;

  const languages = languagesData?.items ?? [];
  const languageOptions = languages.map((language) => ({ code: language.code, name: language.name }));
  const defaultLanguageCode = languages.find((language) => language.isDefault)?.code;

  const tabs: TabItem[] = [
    { key: 'general', label: 'Genel', content: <FormDefinitionGeneralForm formId={formId} detail={detail} onSaved={onSaved} /> },
    {
      key: 'translations',
      label: 'Çeviriler',
      content: (
        <FormDefinitionTranslationForm formId={formId} detail={detail} languages={languageOptions} defaultLanguageCode={defaultLanguageCode} onSaved={onSaved} />
      ),
    },
    { key: 'fields', label: 'Alanlar', content: <FormFieldsPanel formId={formId} detail={detail} languages={languageOptions} onSaved={onSaved} /> },
  ];

  return (
    <div>
      <p>
        <Link to={ROUTES.websiteForms} className={buttonClassName('quiet')}>
          Listeye dön
        </Link>{' '}
        {detail.isActive ? (
          <Button tone="secondary" onClick={() => deactivateMutation.mutate()} disabled={deactivateMutation.isPending}>
            Pasifleştir
          </Button>
        ) : (
          <Button onClick={() => activateMutation.mutate()} disabled={activateMutation.isPending}>
            Etkinleştir
          </Button>
        )}{' '}
        <Button tone="danger" onClick={() => setIsDeleteOpen(true)}>
          Formu sil
        </Button>
      </p>
      <Tabs items={tabs} activeKey={activeTab} onChange={setActiveTab} />
      <ConfirmDialog
        open={isDeleteOpen}
        title="Formu sil"
        description="Bu form tanımı kalıcı olarak silinecek. Bu forma ait en az bir başvuru varsa backend silmeyi reddeder."
        isConfirming={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate()}
        onCancel={() => setIsDeleteOpen(false)}
      />
    </div>
  );
}

export default FormDefinitionDetailPage;
