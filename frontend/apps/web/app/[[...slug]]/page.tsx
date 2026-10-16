import { notFound, permanentRedirect, redirect } from 'next/navigation';
import type { Metadata } from 'next';
import { RouteResolutionKind } from '@gm/api-client';
import { getPublicHome } from '@gm/api-client';
import { createServerApiClient } from '../../lib/apiClient';
import { pathFromSlug, resolvePage } from '../../lib/resolvePage';
import { metadataFromHomeSeo, metadataFromResolution } from '../../lib/seo';
import { HomePage } from '../HomePage';

interface CatchAllPageProps {
  params: Promise<{ slug?: string[] }>;
}

export async function generateMetadata({ params }: CatchAllPageProps): Promise<Metadata> {
  const path = pathFromSlug((await params).slug);
  const resolution = await resolvePage(path);

  if (resolution.kind === RouteResolutionKind.Home) {
    const home = await getPublicHome(createServerApiClient());
    return metadataFromHomeSeo(home);
  }

  if (resolution.kind === RouteResolutionKind.Detail || resolution.kind === RouteResolutionKind.Listing) {
    return metadataFromResolution(resolution, path);
  }

  return {};
}

export default async function CatchAllPage({ params }: CatchAllPageProps) {
  const path = pathFromSlug((await params).slug);
  const resolution = await resolvePage(path);

  switch (resolution.kind) {
    case RouteResolutionKind.Home:
      return <HomePage />;

    case RouteResolutionKind.Redirect: {
      const target = resolution.location ?? '/';
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
    case RouteResolutionKind.Listing:
      // Koşu 3 kapsamı (içerik şablonları). Mock veri üretmek yerine dürüst bir "henüz yok"
      // durumu - gerçek bir içerik asla bu ekranı görmemeli, görürse bu bir regresyondur.
      return (
        <main className="container section">
          <p className="eyebrow">{resolution.kind === RouteResolutionKind.Detail ? 'İçerik' : 'Liste'}</p>
          <p className="page-lead">Bu sayfa şablonu henüz hazır değil (Faz 3 Koşu 3).</p>
        </main>
      );

    case RouteResolutionKind.NotFound:
    default:
      notFound();
  }
}
