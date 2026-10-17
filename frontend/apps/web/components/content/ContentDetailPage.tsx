import { getPublicContentById } from '@gm/api-client';
import { SafeHtml, JsonLd, Breadcrumb, formatDateTime } from '@gm/ui';
import { createServerApiClient } from '../../lib/apiClient';
import { getSite } from '../../lib/getSite';
import { BlockImage } from '../blocks/BlockImage';
import { renderLayoutBlocks } from '../blocks/BlockRegistry';
import { EventInfoPanel } from '../events/EventInfoPanel';
import { EventRegistrationSection } from '../events/EventRegistrationSection';
import { FormRenderer } from '../forms/FormRenderer';
import { GallerySection } from './GallerySection';
import { VideoListSection } from './VideoListSection';
import { AttachmentListSection } from './AttachmentListSection';
import { RelatedContentSection } from './RelatedContentSection';
import { ChildrenSection } from './ChildrenSection';
import { ShareButtons } from './ShareButtons';

/**
 * Genel içerik detay şablonu (ADR-024 §4.1 `DetailTemplate`: `page/article/project/story/event/
 * opportunity`). Bu alt tiplerin tamamı aynı `PublicContentDetailResponse` şeklini paylaşır;
 * fark yalnız hangi bölümlerin dolu geldiğidir (`event`/`form`/`blocks`/`children` null ise o
 * bölüm hiç render edilmez) - bu yüzden tek bir bileşen yeterlidir, şablon başına ayrı dosya yok.
 */
export async function ContentDetailPage({ contentItemId, nonce }: { contentItemId: string; nonce?: string }) {
  const client = createServerApiClient();
  const [detail, site] = await Promise.all([getPublicContentById(client, contentItemId), getSite()]);

  const heroImage = detail.detailImage ?? detail.coverImage;
  const jsonLd = detail.jsonLd.length > 0 ? detail.jsonLd : null;

  return (
    <article className="container section content-detail">
      {jsonLd && jsonLd.map((entry, index) => <JsonLd key={index} data={entry} nonce={nonce} />)}
      <Breadcrumb items={detail.breadcrumb} />
      <header className="content-detail__header">
        <h1>{detail.title}</h1>
        <p className="content-detail__date">{formatDateTime(detail.effectivePublishDate)}</p>
        {detail.categories.length > 0 && (
          <ul className="content-detail__categories">
            {detail.categories.map((category) => (
              <li key={category.id}>{category.name}</li>
            ))}
          </ul>
        )}
      </header>

      {heroImage && (
        <BlockImage image={heroImage} alt={detail.title} sizes="(min-width: 900px) 70vw, 100vw" className="content-detail__hero" priority />
      )}

      {detail.event && (
        <>
          <EventInfoPanel event={detail.event} />
          <EventRegistrationSection
            contentItemId={detail.id}
            initialEvent={detail.event}
            turnstileSiteKey={site.turnstileSiteKey}
            privacyNotice={site.eventPrivacyNotice}
          />
        </>
      )}

      {detail.summary && <p className="content-detail__summary">{detail.summary}</p>}
      {detail.body && <SafeHtml className="content-detail__body" html={detail.body} />}

      {detail.blocks && detail.blocks.length > 0 && (
        <div className="content-detail__blocks">{renderLayoutBlocks(detail.blocks, { publicJobListingsEnabled: site.publicJobListingsEnabled })}</div>
      )}

      <GallerySection gallery={detail.gallery} />
      <VideoListSection videos={detail.videos} />
      <AttachmentListSection attachments={detail.attachments} />

      {detail.form && <FormRenderer formKey={detail.form.key} definition={detail.form} contentItemId={detail.id} turnstileSiteKey={site.turnstileSiteKey} />}

      <ChildrenSection children={detail.children} />
      <RelatedContentSection items={detail.related} />

      <ShareButtons title={detail.title} />
    </article>
  );
}
