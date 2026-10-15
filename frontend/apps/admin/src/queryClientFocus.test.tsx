import { AuthProvider } from '@gm/auth';
import { ToastProvider } from '@gm/ui';
import { QueryClientProvider, focusManager } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { createQueryClient } from './App';
import { ContentItemDetailPage } from './features/website/contentItems/ContentItemDetailPage';
import { HomeLayoutPage } from './features/website/layouts/HomeLayoutPage';
import { MediaLibraryPage } from './features/media/MediaLibraryPage';

const CONTENT_TYPE_ID = '11111111-1111-1111-1111-111111111111';
const CONTENT_ITEM_ID = '22222222-2222-2222-2222-222222222222';

function jsonResponse(body: unknown) {
  return new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } });
}

function pagedEmpty() {
  return { items: [], totalCount: 0, page: 1, pageSize: 24, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
}

const LANGUAGES = { items: [{ id: 'lang-1', code: 'tr', name: 'Türkçe', isDefault: true, isActive: true, sortOrder: 0 }] };

const HOME_LAYOUT = { draftBlocks: [], publishedBlocks: [], hasUnpublishedChanges: false, publishedAtUtc: null, rowVersion: 'rv-home' };

const CONTENT_ITEM_DETAIL = {
  id: CONTENT_ITEM_ID,
  contentTypeId: CONTENT_TYPE_ID,
  parentId: null,
  status: 'Draft',
  publishAtUtc: null,
  unpublishAtUtc: null,
  sortOrder: 0,
  isFeatured: false,
  coverImageMediaId: null,
  coverImageUrl: null,
  detailImageMediaId: null,
  detailImageUrl: null,
  formDefinitionId: null,
  isVisible: true,
  rowVersion: 'rv-item',
  categoryIds: [],
  galleryItems: [],
  videoIds: [],
  attachments: [],
  relatedContentItemIds: [],
  translations: [
    {
      languageCode: 'tr',
      title: 'Test başlık',
      slug: 'test-baslik',
      fullPath: 'test-baslik',
      summary: '',
      body: '<p></p>',
      seo: { metaTitle: '', metaDescription: '', metaKeywords: '', ogTitle: '', ogDescription: '', ogImageMediaId: null, canonicalUrl: null, noIndex: false },
      tagIds: [],
    },
  ],
};

const CONTENT_TYPE_DETAIL = {
  id: CONTENT_TYPE_ID,
  key: 'test-type',
  listTemplate: 'default',
  detailTemplate: 'default',
  sortMode: 'Manual',
  isActive: true,
  sortOrder: 0,
  supportsHierarchy: false,
  supportsCategories: false,
  supportsTags: true,
  supportsDetailImage: false,
  supportsGallery: false,
  supportsVideos: false,
  supportsAttachments: false,
  supportsEvent: false,
  supportsBlockLayout: false,
  supportsForm: false,
  supportsRelatedContent: false,
  hasDetailPage: true,
  hasListingPage: true,
  isSearchable: false,
  requiresReview: false,
  schemaKind: 'None',
  rowVersion: 'rv-type',
  translations: [],
};

/**
 * Yola göre yanıt döner (sorgu dizesi yok sayılır - bu testler istek SAYISINI doğruluyor, parametre
 * ayrıntılarını değil). Eşleşmeyen bir yol çağrılırsa gerçek bir API'ye değişmişiz anlamına gelir -
 * sessizce 404 döner, test bunu çağrı SAYISI üzerinden (`toBe`) zaten yakalar.
 */
function createRoutedFetchMock(routes: Record<string, unknown>) {
  return vi.fn((input: string | URL | Request) => {
    const url = typeof input === 'string' ? input : input instanceof Request ? input.url : input.toString();
    const pathname = new URL(url).pathname;
    if (pathname in routes) {
      return Promise.resolve(jsonResponse(routes[pathname]));
    }
    return Promise.resolve(new Response(null, { status: 404 }));
  });
}

/**
 * React Query'nin `FocusManager`'ını doğrudan sürer (kütüphanenin kendi önerdiği test yöntemi) -
 * jsdom'da gerçek pencere blur/focus olaylarını güvenilir biçimde tetiklemenin bir yolu yok.
 */
async function simulateWindowRefocus() {
  focusManager.setFocused(false);
  focusManager.setFocused(true);
  await new Promise((resolve) => setTimeout(resolve, 0));
}

describe('admin QueryClient varsayılanları: pencere odağına dönüşte yeniden istek atılmaz', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    focusManager.setFocused(undefined);
  });

  it('Ana sayfa düzeni: odağa dönüş languages/block-types/layout sorgularını yeniden tetiklemez', async () => {
    const fetchMock = createRoutedFetchMock({
      '/api/v1/admin/website/languages': LANGUAGES,
      '/api/v1/admin/website/block-types': [],
      '/api/v1/admin/website/layouts/home': HOME_LAYOUT,
    });
    vi.stubGlobal('fetch', fetchMock);

    render(
      <QueryClientProvider client={createQueryClient()}>
        <AuthProvider baseUrl="http://test.local">
          <ToastProvider>
            <HomeLayoutPage />
          </ToastProvider>
        </AuthProvider>
      </QueryClientProvider>,
    );

    await waitFor(() => expect(screen.getByRole('button', { name: 'Taslağı kaydet' })).toBeInTheDocument());
    // Üç başlangıç sorgusu (languages, block-types, layout) - tam olarak bu sayı: fazlası beklenmeyen
    // bir yeniden sorgu, azı henüz yarım kalmış bir yükleme olurdu.
    await waitFor(() => expect(fetchMock.mock.calls.length).toBe(3));

    await simulateWindowRefocus();

    expect(fetchMock.mock.calls.length).toBe(3);
  });

  it('İçerik detayı: odağa dönüş content/content-type/languages/tags sorgularını yeniden tetiklemez', async () => {
    const fetchMock = createRoutedFetchMock({
      [`/api/v1/admin/website/contents/${CONTENT_ITEM_ID}`]: CONTENT_ITEM_DETAIL,
      [`/api/v1/admin/website/content-types/${CONTENT_TYPE_ID}`]: CONTENT_TYPE_DETAIL,
      '/api/v1/admin/website/languages': LANGUAGES,
      '/api/v1/admin/website/tags': pagedEmpty(),
    });
    vi.stubGlobal('fetch', fetchMock);

    render(
      <MemoryRouter initialEntries={[`/site-yonetimi/icerikler/${CONTENT_ITEM_ID}`]}>
        <QueryClientProvider client={createQueryClient()}>
          <AuthProvider baseUrl="http://test.local">
            <ToastProvider>
              <Routes>
                <Route path="/site-yonetimi/icerikler/:contentItemId" element={<ContentItemDetailPage />} />
              </Routes>
            </ToastProvider>
          </AuthProvider>
        </QueryClientProvider>
      </MemoryRouter>,
    );

    // `getByLabelText` zengin metin araç çubuğundaki "Başlık 2"/"Başlık 3" aria-label'larıyla da
    // eşleşip belirsizleşiyor (`RichTextEditor` aynı panelde yüklü) - alanın kendi değeriyle ara.
    await waitFor(() => expect(screen.getByDisplayValue('Test başlık')).toBeInTheDocument());
    // Dört başlangıç sorgusu: contents, content-types (contents çözülünce), languages, tags
    // (ContentItemGeneralForm monte olunca) - sırayla açılan dalgalar halinde gelir.
    await waitFor(() => expect(fetchMock.mock.calls.length).toBe(4));

    await simulateWindowRefocus();

    expect(fetchMock.mock.calls.length).toBe(4);
  });

  it('Medya kütüphanesi: odağa dönüş medya listesi sorgusunu yeniden tetiklemez', async () => {
    const fetchMock = createRoutedFetchMock({
      '/api/v1/admin/website/media': pagedEmpty(),
    });
    vi.stubGlobal('fetch', fetchMock);

    render(
      <QueryClientProvider client={createQueryClient()}>
        <AuthProvider baseUrl="http://test.local">
          <ToastProvider>
            <MediaLibraryPage />
          </ToastProvider>
        </AuthProvider>
      </QueryClientProvider>,
    );

    await waitFor(() => expect(screen.getByRole('button', { name: 'Medya yükle' })).toBeInTheDocument());
    await waitFor(() => expect(fetchMock.mock.calls.length).toBe(1));

    await simulateWindowRefocus();

    expect(fetchMock.mock.calls.length).toBe(1);
  });

  it('refetchOnWindowFocus: false, veri taze olmasa (staleTime 0) bile odağa dönüşte yeniden sorgu atmaz', async () => {
    // Yukarıdaki üç test gerçek uygulamanın 30 sn'lik `staleTime`'ıyla çalışıyor - bu süre içinde
    // zaten odak değişimi ne olursa olsun yeniden sorgu atılmaz, dolayısıyla tek başına
    // `refetchOnWindowFocus: false`'un etkisini kanıtlamazlar (biri bu satırı silse de o üç test
    // yine geçerdi - sınandı). Burada `staleTimeMs: 0` ile veriyi anında "bayat" sayarak
    // `refetchOnWindowFocus: false` olmasaydı odağa dönüşün gerçekten yeniden sorgu tetikleyeceğini
    // (ve false olunca tetiklemediğini) izole ediyoruz.
    const fetchMock = createRoutedFetchMock({ '/api/v1/admin/website/media': pagedEmpty() });
    vi.stubGlobal('fetch', fetchMock);

    render(
      <QueryClientProvider client={createQueryClient({ staleTimeMs: 0 })}>
        <AuthProvider baseUrl="http://test.local">
          <ToastProvider>
            <MediaLibraryPage />
          </ToastProvider>
        </AuthProvider>
      </QueryClientProvider>,
    );

    await waitFor(() => expect(screen.getByRole('button', { name: 'Medya yükle' })).toBeInTheDocument());
    await waitFor(() => expect(fetchMock.mock.calls.length).toBe(1));

    await simulateWindowRefocus();

    expect(fetchMock.mock.calls.length).toBe(1);
  });
});
