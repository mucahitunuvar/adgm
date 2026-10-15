import {
  activateContentType,
  deactivateContentType,
  getApiErrorMessage,
  getContentTypeById,
  getSiteLanguages,
} from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, buttonClassName, ErrorState, LoadingState, PageHeader, Tabs, type TabItem, useToast } from '@gm/ui';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { ContentCategoriesPanel } from './ContentCategoriesPanel';
import { ContentTypeGeneralForm } from './ContentTypeGeneralForm';
import { ContentTypeTranslationForm } from './ContentTypeTranslationForm';

export function ContentTypeDetailPage() {
  const { contentTypeId } = useParams<{ contentTypeId: string }>();
  const { client } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('general');

  const queryKey = ['website-content-type', contentTypeId] as const;
  const { data: detail, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getContentTypeById(client, contentTypeId!),
    enabled: Boolean(contentTypeId),
  });
  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });

  function onSaved() {
    void queryClient.invalidateQueries({ queryKey });
  }

  const activateMutation = useMutation({
    mutationFn: () => activateContentType(client, contentTypeId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('İçerik tipi etkinleştirildi.', 'success');
      onSaved();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });
  const deactivateMutation = useMutation({
    mutationFn: () => deactivateContentType(client, contentTypeId!, { rowVersion: detail!.rowVersion }),
    onSuccess: () => {
      showToast('İçerik tipi pasifleştirildi.', 'success');
      onSaved();
    },
    onError: (mutationError) => showToast(getApiErrorMessage(mutationError), 'danger'),
  });

  if (!contentTypeId) return <ErrorState description="İçerik tipi kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !detail) return <ErrorState description={error ? getApiErrorMessage(error) : 'İçerik tipi bulunamadı.'} />;

  const languages = languagesData?.items ?? [];
  const languageOptions = languages.map((language) => ({ code: language.code, name: language.name }));
  const defaultLanguageCode = languages.find((language) => language.isDefault)?.code;

  const tabs: TabItem[] = [
    { key: 'general', label: 'Genel', content: <ContentTypeGeneralForm detail={detail} onSaved={onSaved} /> },
    {
      key: 'translations',
      label: 'Çeviriler',
      content: (
        <ContentTypeTranslationForm
          contentTypeId={contentTypeId}
          detail={detail}
          languages={languageOptions}
          defaultLanguageCode={defaultLanguageCode}
          onSaved={onSaved}
        />
      ),
    },
    ...(detail.supportsCategories
      ? [{ key: 'categories', label: 'Kategoriler', content: <ContentCategoriesPanel contentTypeId={contentTypeId} languages={languageOptions} /> }]
      : []),
  ];

  return (
    <div className="container section">
      <PageHeader
        eyebrow="İçerik tipleri"
        title={detail.translations.find((t) => t.languageCode === defaultLanguageCode)?.name ?? detail.key}
        description={`Anahtar: ${detail.key}`}
        actions={
          <>
            <Link to={ROUTES.websiteContentTypes} className={buttonClassName('quiet')}>
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
            )}
          </>
        }
      />
      <Tabs items={tabs} activeKey={activeTab} onChange={setActiveTab} />
    </div>
  );
}
