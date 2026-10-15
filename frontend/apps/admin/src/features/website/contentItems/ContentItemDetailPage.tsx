import { getApiErrorMessage, getContentItemById, getContentTypeById, getSiteLanguages } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { ErrorState, LoadingState, PageHeader, Tabs, buttonClassName, type TabItem } from '@gm/ui';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ROUTES } from '../../../routes';
import { ContentItemAttachmentsPanel } from './ContentItemAttachmentsPanel';
import { ContentItemCategoriesPanel } from './ContentItemCategoriesPanel';
import { ContentItemEventPanel } from './ContentItemEventPanel';
import { ContentItemGalleryPanel } from './ContentItemGalleryPanel';
import { ContentItemGeneralForm } from './ContentItemGeneralForm';
import { ContentItemHierarchyPanel } from './ContentItemHierarchyPanel';
import { ContentItemLifecycleActions } from './ContentItemLifecycleActions';
import { ContentItemRelatedContentPanel } from './ContentItemRelatedContentPanel';
import { ContentItemRevisionsPanel } from './ContentItemRevisionsPanel';
import { ContentItemVideosPanel } from './ContentItemVideosPanel';
import { ContentLayoutPanel } from '../layouts/ContentLayoutPanel';

export function ContentItemDetailPage() {
  const { contentItemId } = useParams<{ contentItemId: string }>();
  const { client } = useAuth();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('general');

  const queryKey = ['website-content-item', contentItemId] as const;
  const { data: detail, isLoading, error } = useQuery({
    queryKey,
    queryFn: () => getContentItemById(client, contentItemId!),
    enabled: Boolean(contentItemId),
  });

  const { data: contentType } = useQuery({
    queryKey: ['website-content-type', detail?.contentTypeId],
    queryFn: () => getContentTypeById(client, detail!.contentTypeId),
    enabled: Boolean(detail),
  });

  const { data: languagesData } = useQuery({ queryKey: ['website-languages'], queryFn: () => getSiteLanguages(client) });

  function onSaved() {
    void queryClient.invalidateQueries({ queryKey });
  }

  if (!contentItemId) return <ErrorState description="İçerik kimliği eksik." />;
  if (isLoading) return <LoadingState />;
  if (error || !detail) return <ErrorState description={error ? getApiErrorMessage(error) : 'İçerik bulunamadı.'} />;
  if (!contentType) return <LoadingState />;

  const languages = languagesData?.items ?? [];
  const languageOptions = languages.map((language) => ({ code: language.code, name: language.name }));
  const defaultLanguageCode = languages.find((language) => language.isDefault)?.code ?? languageOptions[0]?.code ?? '';
  const title = detail.translations.find((t) => t.languageCode === defaultLanguageCode)?.title ?? detail.translations[0]?.title ?? contentItemId;

  const tabs: TabItem[] = [
    {
      key: 'general',
      label: 'Genel',
      content: (
        <ContentItemGeneralForm
          contentItemId={contentItemId}
          detail={detail}
          contentType={contentType}
          languages={languageOptions}
          defaultLanguageCode={defaultLanguageCode}
          onSaved={onSaved}
        />
      ),
    },
  ];

  if (contentType.supportsHierarchy) {
    tabs.push({ key: 'hierarchy', label: 'Hiyerarşi', content: <ContentItemHierarchyPanel contentItemId={contentItemId} detail={detail} onSaved={onSaved} /> });
  }
  if (contentType.supportsCategories) {
    tabs.push({
      key: 'categories',
      label: 'Kategoriler',
      content: <ContentItemCategoriesPanel contentItemId={contentItemId} contentTypeId={detail.contentTypeId} detail={detail} onSaved={onSaved} />,
    });
  }
  if (contentType.supportsGallery) {
    tabs.push({
      key: 'gallery',
      label: 'Galeri',
      content: <ContentItemGalleryPanel contentItemId={contentItemId} detail={detail} defaultLanguageCode={defaultLanguageCode} onSaved={onSaved} />,
    });
  }
  if (contentType.supportsVideos) {
    tabs.push({ key: 'videos', label: 'Videolar', content: <ContentItemVideosPanel contentItemId={contentItemId} detail={detail} onSaved={onSaved} /> });
  }
  if (contentType.supportsAttachments) {
    tabs.push({
      key: 'attachments',
      label: 'Ek dosyalar',
      content: <ContentItemAttachmentsPanel contentItemId={contentItemId} detail={detail} defaultLanguageCode={defaultLanguageCode} onSaved={onSaved} />,
    });
  }
  if (contentType.supportsRelatedContent) {
    tabs.push({
      key: 'related',
      label: 'İlişkili içerik',
      content: <ContentItemRelatedContentPanel contentItemId={contentItemId} detail={detail} onSaved={onSaved} />,
    });
  }
  if (contentType.supportsEvent) {
    tabs.push({ key: 'event', label: 'Etkinlik', content: <ContentItemEventPanel contentItemId={contentItemId} languages={languageOptions} /> });
  }
  if (contentType.supportsBlockLayout) {
    tabs.push({ key: 'layout', label: 'Düzen', content: <ContentLayoutPanel contentItemId={contentItemId} /> });
  }
  tabs.push({
    key: 'revisions',
    label: 'Revizyonlar',
    content: <ContentItemRevisionsPanel contentItemId={contentItemId} defaultLanguageCode={defaultLanguageCode} onRestored={onSaved} />,
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title={title}
        description={`Yol: ${detail.translations.find((t) => t.languageCode === defaultLanguageCode)?.fullPath ?? ''}`}
        actions={
          <Link to={ROUTES.websiteContentItems} className={buttonClassName('quiet')}>
            Listeye dön
          </Link>
        }
      />
      <ContentItemLifecycleActions contentItemId={contentItemId} detail={detail} onChanged={onSaved} />
      <Tabs items={tabs} activeKey={activeTab} onChange={setActiveTab} />
    </div>
  );
}

export default ContentItemDetailPage;
