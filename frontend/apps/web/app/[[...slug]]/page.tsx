import { headers } from 'next/headers';
import { notFound, permanentRedirect, redirect } from 'next/navigation';
import type { Metadata } from 'next';
import { RouteResolutionKind, getPublicContentById, listPublicContents } from '@gm/api-client';
import { getPublicHome } from '@gm/api-client';
import { createServerApiClient } from '../../lib/apiClient';
import { pathFromSlug, resolvePage } from '../../lib/resolvePage';
import { safeRedirectTarget } from '../../lib/safeRedirectTarget';
import { metadataFromHomeSeo, metadataFromResolution, metadataFromContentDetail, metadataFromContentList } from '../../lib/seo';
import { HomePage } from '../HomePage';
import { ContentDetailPage } from '../../components/content/ContentDetailPage';
import { ContentListPage } from '../../components/content/ContentListPage';
import { EventListPage } from '../../components/events/EventListPage';

interface CatchAllPageProps {
  params: Promise<{ slug?: string[] }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}

function firstParam(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

export async function generateMetadata({ params, searchParams }: CatchAllPageProps): Promise<Metadata> {
  const path = pathFromSlug((await params).slug);
  const resolution = await resolvePage(path);
  const client = createServerApiClient();

  if (resolution.kind === RouteResolutionKind.Home) {
    const home = await getPublicHome(client);
    return metadataFromHomeSeo(home);
  }

  if (resolution.kind === RouteResolutionKind.Detail && resolution.contentItemId) {
    const detail = await getPublicContentById(client, resolution.contentItemId);
    return metadataFromContentDetail(detail);
  }

  if (resolution.kind === RouteResolutionKind.Listing && resolution.contentTypeKey && resolution.detailTemplate !== 'event') {
    const { category, search } = await searchParams;
    const list = await listPublicContents(client, { type: resolution.contentTypeKey, category: firstParam(category), search: firstParam(search) });
    return metadataFromContentList(list, path);
  }

  if (resolution.kind === RouteResolutionKind.Detail || resolution.kind === RouteResolutionKind.Listing) {
    return metadataFromResolution(resolution, path);
  }

  return {};
}

export default async function CatchAllPage({ params, searchParams }: CatchAllPageProps) {
  const path = pathFromSlug((await params).slug);
  const resolution = await resolvePage(path);
  const query = await searchParams;
  const nonce = (await headers()).get('x-nonce') ?? undefined;

  switch (resolution.kind) {
    case RouteResolutionKind.Home:
      return <HomePage />;

    case RouteResolutionKind.Redirect: {
      // Açık yönlendirme koruması: backend'in döndürdüğü `location` kök-göreli bir yol ya da
      // http(s) mutlak URL değilse (ör. bir veri tutarsızlığı veya `Redirects` kaydının beklenmeyen
      // bir şekilde kirlenmesi sonucu) hedef yerine 404 gösterilir - hiçbir zaman doğrulanmamış bir
      // adrese yönlendirme yapılmaz.
      const target = safeRedirectTarget(resolution.location);
      if (!target) notFound();
      // ADR-024 §15: backend 301/302 ayrımını belirler. App Router'ın özel durum fonksiyonları
      // 301/302'yi birebir üretemez (yöntem koruyan 308/307 üretirler); arama motorları 308'i
      // 301 gibi, 307'yi 302 gibi işler - en yakın eşdeğer budur (Route Handler yazmadan).
      if (Number(resolution.statusCode) === 301) {
        permanentRedirect(target);
      }
      redirect(target);
      break;
    }

    case RouteResolutionKind.Detail:
      if (!resolution.contentItemId) notFound();
      return <ContentDetailPage contentItemId={resolution.contentItemId} nonce={nonce} />;

    case RouteResolutionKind.Listing: {
      if (!resolution.contentTypeKey) notFound();
      // Etkinlik türleri (`detailTemplate === 'event'`) canlı kayıt durumu için ayrı, özel bir
      // liste ucu kullanır (ADR-024 §11.3) - genel içerik listesi burada hiç çağrılmaz.
      if (resolution.detailTemplate === 'event') {
        return <EventListPage contentTypeKey={resolution.contentTypeKey} basePath={path} when={firstParam(query.when)} page={Number(firstParam(query.page)) || 1} />;
      }
      return (
        <ContentListPage
          contentTypeKey={resolution.contentTypeKey}
          basePath={path}
          category={firstParam(query.category)}
          search={firstParam(query.search)}
          page={Number(firstParam(query.page)) || 1}
          nonce={nonce}
        />
      );
    }

    case RouteResolutionKind.NotFound:
    default:
      notFound();
  }
}
