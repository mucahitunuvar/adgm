import { test, expect, type APIRequestContext } from '@playwright/test';

const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';

interface MenuItemLink {
  kind: string;
  contentItemId: string | null;
  contentTypeId: string | null;
  internalPath: string | null;
  externalUrl: string | null;
}
interface MenuItemTranslation {
  languageCode: string;
  label: string;
}
interface MenuItemResponse {
  id: string;
  parentId: string | null;
  sortOrder: number;
  isActive: boolean;
  link: MenuItemLink | null;
  openInNewTab: boolean;
  iconKey: string | null;
  translations: MenuItemTranslation[];
  brokenLinkReason: string | null;
}
interface MenuItemTreeInput {
  tempId: string;
  parentTempId: string | null;
  sortOrder: number;
  isActive: boolean;
  link: MenuItemLink | null;
  openInNewTab: boolean;
  iconKey: string | null;
  translations: MenuItemTranslation[];
}
interface MenuResponse {
  id: string;
  location: string;
  rowVersion: string;
  items: MenuItemResponse[];
}
interface LayoutBlock {
  blockTypeKey: string;
  sortOrder: number;
  isActive: boolean;
  settings: unknown;
  texts: Record<string, never> | null;
}
interface HomeLayoutResponse {
  draftBlocks: LayoutBlock[];
  publishedBlocks: LayoutBlock[];
  hasUnpublishedChanges: boolean;
  publishedAtUtc: string | null;
  rowVersion: string;
}
interface SiteLanguage {
  code: string;
  isActive: boolean;
}

async function loginForApiToken(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, { data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD } });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

// Var olan her öğe kendi gerçek `id`'sini `tempId` olarak kullanır (bkz. adminWebsiteMenus.ts'teki
// not) - `parentId` zaten bu kümedeki bir `id`'ye işaret ettiğinden `parentTempId` doğrudan eşlenir.
function menuItemsToTreeInput(items: MenuItemResponse[]): MenuItemTreeInput[] {
  return items.map((item) => ({
    tempId: item.id,
    parentTempId: item.parentId,
    sortOrder: item.sortOrder,
    isActive: item.isActive,
    link: item.link,
    openInNewTab: item.openInNewTab,
    iconKey: item.iconKey,
    translations: item.translations,
  }));
}

function blockToInput(block: LayoutBlock) {
  return { blockTypeKey: block.blockTypeKey, sortOrder: block.sortOrder, isActive: block.isActive, settings: block.settings, texts: block.texts };
}

test.describe.configure({ mode: 'serial' });

test.describe('admin: menü + slider + ana sayfa düzeni yayın zinciri (gerçek backend, canlı veriyi bozmaz)', () => {
  let headers: Record<string, string>;
  let originalHeaderMenu: MenuResponse | undefined;
  let originalHomeLayout: HomeLayoutResponse | undefined;
  let createdSliderId: string | undefined;
  let createdMediaId: string | undefined;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request);
    headers = { Authorization: `Bearer ${token}` };
  });

  // Ana test zaten kendi içinde yedekleyip geri alıyor (bkz. aşağıdaki test gövdesinin 7-9. adımları);
  // bu `afterAll` yalnız test YARIDA başarısız olursa devreye giren bir güvenlik ağıdır. Plan madde 3
  // "geri yükleme başarısız olursa DUR ve bildir" gereği - burada da bir `expect` başarısız olursa hook
  // FAILED raporlanır, sessizce yutulmaz.
  test.afterAll(async ({ request }) => {
    if (originalHomeLayout) {
      const current = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
      const stillHasTestBlock = current.draftBlocks.some(
        (b) => b.blockTypeKey === 'hero-slider' && (b.settings as { sliderId?: string } | null)?.sliderId === createdSliderId,
      );
      if (stillHasTestBlock) {
        const restoreDraft = await request.put(`${BACKEND_URL}/api/v1/admin/website/layouts/home/draft`, {
          headers,
          data: { rowVersion: current.rowVersion, blocks: originalHomeLayout.publishedBlocks.map(blockToInput) },
        });
        expect(restoreDraft.status(), 'ana sayfa taslağı güvenlik ağında geri alınamadı').toBe(204);
        const afterRestoreDraft = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
        const restorePublish = await request.post(`${BACKEND_URL}/api/v1/admin/website/layouts/home/publish`, {
          headers,
          data: { rowVersion: afterRestoreDraft.rowVersion },
        });
        expect(restorePublish.status(), 'ana sayfa düzeni güvenlik ağında yeniden yayınlanamadı').toBe(204);
      }
    }

    if (originalHeaderMenu) {
      const current = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/menus`, { headers })).json()) as MenuResponse[];
      const header = current.find((m) => m.location === originalHeaderMenu!.location);
      const stillHasTestItem = header?.items.some((i) => i.translations.some((t) => t.label.startsWith('e2e-')));
      if (header && stillHasTestItem) {
        const restoreMenu = await request.put(`${BACKEND_URL}/api/v1/admin/website/menus/${header.location}`, {
          headers,
          data: { rowVersion: header.rowVersion, items: menuItemsToTreeInput(originalHeaderMenu.items) },
        });
        expect(restoreMenu.status(), 'Header menüsü güvenlik ağında geri alınamadı').toBe(204);
      }
    }

    // Slider ANCAK ana sayfa düzeni artık ona referans vermiyorsa silinebilir (409: kullanımda) -
    // bu yüzden bu sıradan SONRA.
    if (createdSliderId) {
      const deleteSliderResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/sliders/${createdSliderId}`, { headers });
      expect([204, 404], 'e2e slider güvenlik ağında silinemedi').toContain(deleteSliderResponse.status());
    }

    if (createdMediaId) {
      const deleteMediaResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/media/${createdMediaId}`, { headers });
      expect([204, 404], 'e2e medya güvenlik ağında silinemedi').toContain(deleteMediaResponse.status());
    }
  });

  test('menü öğesi + slider slaytı + ana sayfa hero-slider bloğu yayınlanınca public/site ve public/home\'da görünür, geri alınca kaybolur', async ({
    request,
  }) => {
    test.setTimeout(60_000);

    // ---- 1. Yedekleme: şu an YAYINDA olan menü ve ana sayfa düzeni ----
    const menusBefore = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/menus`, { headers })).json()) as MenuResponse[];
    const headerMenu = menusBefore.find((m) => m.location === 'Header');
    expect(headerMenu, 'Header konumunda menü bulunamadı - dev veritabanı kurulumu beklenenden farklı').toBeTruthy();
    originalHeaderMenu = headerMenu;

    const homeLayoutBefore = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
    originalHomeLayout = homeLayoutBefore;

    const languagesResponse = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/languages`, { headers })).json()) as { items: SiteLanguage[] };
    const activeLanguageCodes = languagesResponse.items.filter((l) => l.isActive).map((l) => l.code);
    expect(activeLanguageCodes.length).toBeGreaterThan(0);

    // ---- 2. Medya: mevcut bir varlığı yeniden kullan, yoksa 1x1 e2e PNG yükle (test sonunda silinir) ----
    const mediaList = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/media`, { headers, params: { pageSize: 1 } })
    ).json()) as { items: Array<{ id: string }> };
    let imageMediaId: string;
    if (mediaList.items.length > 0) {
      imageMediaId = mediaList.items[0]!.id;
    } else {
      const png1x1 = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=', 'base64');
      const uploadResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/media`, {
        headers,
        multipart: { file: { name: 'e2e-fixture.png', mimeType: 'image/png', buffer: png1x1 }, altText: 'e2e-fixture' },
      });
      expect(uploadResponse.status()).toBe(201);
      const uploaded = (await uploadResponse.json()) as { id: string };
      createdMediaId = uploaded.id;
      imageMediaId = uploaded.id;
    }

    // ---- 3. Slider: tamamen bize ait, e2e- önekli, test sonunda silinir ----
    const sliderKey = `e2e-slider-${Date.now()}`;
    const createSliderResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/sliders`, {
      headers,
      data: { key: sliderKey, defaultLanguageName: 'E2E Slider' },
    });
    expect(createSliderResponse.status()).toBe(201);
    const createdSlider = (await createSliderResponse.json()) as { id: string; defaultLanguageCode: string };
    createdSliderId = createdSlider.id;

    const sliderDetail = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/sliders/${createdSliderId}`, { headers })
    ).json()) as { rowVersion: string };
    const slideTitle = `E2E Slayt ${Date.now()}`;
    const replaceSlidesResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/sliders/${createdSliderId}/slides`, {
      headers,
      data: {
        rowVersion: sliderDetail.rowVersion,
        slides: [
          {
            desktopImageMediaId: imageMediaId,
            mobileImageMediaId: null,
            link: { kind: 'None', contentItemId: null, contentTypeId: null, internalPath: null, externalUrl: null },
            sortOrder: 0,
            isActive: true,
            publishAtUtc: null,
            unpublishAtUtc: null,
            translations: [
              {
                languageCode: createdSlider.defaultLanguageCode,
                eyebrow: null,
                title: slideTitle,
                text: null,
                buttonLabel: null,
                altTextOverride: 'e2e slayt görseli',
              },
            ],
          },
        ],
      },
    });
    expect(replaceSlidesResponse.status()).toBe(204);

    // ---- 4. Ana sayfa taslağına hero-slider bloğu ekle, yayınla ----
    const draftWithNewBlock = [
      ...homeLayoutBefore.publishedBlocks.map(blockToInput),
      {
        blockTypeKey: 'hero-slider',
        sortOrder: homeLayoutBefore.publishedBlocks.length,
        isActive: true,
        settings: { sliderId: createdSliderId },
        texts: null,
      },
    ];
    const putDraftResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/layouts/home/draft`, {
      headers,
      data: { rowVersion: homeLayoutBefore.rowVersion, blocks: draftWithNewBlock },
    });
    expect(putDraftResponse.status()).toBe(204);

    const afterDraftPut = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
    const publishResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/layouts/home/publish`, {
      headers,
      data: { rowVersion: afterDraftPut.rowVersion },
    });
    expect(publishResponse.status()).toBe(204);

    // ---- 5. Menü: Header konumuna gerçek bir bağlantıyla e2e öğesi ekle ----
    // NOT: `kind: 'None'` (bağlantısız) bir öğe, alt öğesi yoksa "boş grup başlığı" sayılıp public
    // yanıtından düşürülür (bkz. PublicMenuItemResponse.cs'teki Href notu) - bu yüzden görünürlüğü
    // anlamlı şekilde sınayabilmek için gerçek, her zaman çözümlenen bir dış bağlantı kullanılıyor.
    const menuItemLabel = `e2e-menu-${Date.now()}`;
    const newMenuItem: MenuItemTreeInput = {
      tempId: crypto.randomUUID(),
      parentTempId: null,
      sortOrder: headerMenu!.items.length,
      isActive: true,
      link: { kind: 'ExternalUrl', contentItemId: null, contentTypeId: null, internalPath: null, externalUrl: 'https://example.com/e2e-test' },
      openInNewTab: true,
      iconKey: null,
      translations: activeLanguageCodes.map((code) => ({ languageCode: code, label: menuItemLabel })),
    };
    const newMenuTree: MenuItemTreeInput[] = [...menuItemsToTreeInput(headerMenu!.items), newMenuItem];
    const putMenuResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/menus/${headerMenu!.location}`, {
      headers,
      data: { rowVersion: headerMenu!.rowVersion, items: newMenuTree },
    });
    expect(putMenuResponse.status()).toBe(204);

    // ---- 6. Doğrulama: ikisi de public uçlarda görünüyor ----
    const defaultLang = createdSlider.defaultLanguageCode;
    const publicHomeAfter = (await (
      await request.get(`${BACKEND_URL}/api/v1/public/home`, { params: { lang: defaultLang } })
    ).json()) as { blocks: Array<{ type: string; data: unknown }> };
    // KOŞU 3G Görev 4 (e2e kararlılığı, 17 Ekim): `seed-dev.mjs` ana sayfaya ZATEN kendi hero-slider
    // bloğunu seed'liyor - `.find(b => b.type === 'hero-slider')` seed-dev'in bloğunu (bu testin
    // SONUNA eklediği YENİ blok değil) bulup testi yanlış gerekçeyle düşürüyordu. Adım 9'daki aynı
    // doğrulama ZATEN `slideTitle`'a göre süzüyordu (doğru desen) - burası ona hizalandı.
    const heroBlockAfter = publicHomeAfter.blocks.find(
      (b) => b.type === 'hero-slider' && (b.data as Array<{ title: string }>).some((s) => s.title === slideTitle),
    );
    expect(heroBlockAfter, "yayınlanan hero-slider bloğu public/home'da bulunamadı").toBeTruthy();

    const publicSiteAfter = (await (
      await request.get(`${BACKEND_URL}/api/v1/public/site`, { params: { lang: defaultLang } })
    ).json()) as { menus: { header: Array<{ label: string; href: string | null }> } };
    expect(publicSiteAfter.menus.header.some((i) => i.label === menuItemLabel && i.href === 'https://example.com/e2e-test')).toBe(true);

    // ---- 7. Geri alma: ana sayfa taslağı önceki haline döner (taze rowVersion'la) ----
    const beforeRestoreHome = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
    const restoreDraftResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/layouts/home/draft`, {
      headers,
      data: { rowVersion: beforeRestoreHome.rowVersion, blocks: homeLayoutBefore.publishedBlocks.map(blockToInput) },
    });
    expect(restoreDraftResponse.status()).toBe(204);
    const afterRestoreDraft = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
    const restorePublishResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/layouts/home/publish`, {
      headers,
      data: { rowVersion: afterRestoreDraft.rowVersion },
    });
    expect(restorePublishResponse.status()).toBe(204);

    // ---- 8. Geri alma: Header menüsü önceki haline döner (taze rowVersion'la) ----
    const beforeRestoreMenu = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/menus`, { headers })).json()) as MenuResponse[];
    const headerMenuBeforeRestore = beforeRestoreMenu.find((m) => m.location === headerMenu!.location)!;
    const restoreMenuResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/menus/${headerMenu!.location}`, {
      headers,
      data: { rowVersion: headerMenuBeforeRestore.rowVersion, items: menuItemsToTreeInput(headerMenu!.items) },
    });
    expect(restoreMenuResponse.status()).toBe(204);

    // ---- 9. Doğrulama: ikisi de public uçlarda artık YOK; sunucudaki gerçek hal teste başlamadan
    //         önceki yayınlanmış halle BİREBİR eşleşiyor (yalnız "yok" değil, tam eşitlik). ----
    const publicHomeRestored = (await (
      await request.get(`${BACKEND_URL}/api/v1/public/home`, { params: { lang: defaultLang } })
    ).json()) as { blocks: Array<{ type: string; data: unknown }> };
    expect(
      publicHomeRestored.blocks.some((b) => b.type === 'hero-slider' && (b.data as Array<{ title: string }>).some((s) => s.title === slideTitle)),
    ).toBe(false);

    const publicSiteRestored = (await (
      await request.get(`${BACKEND_URL}/api/v1/public/site`, { params: { lang: defaultLang } })
    ).json()) as { menus: { header: Array<{ label: string }> } };
    expect(publicSiteRestored.menus.header.some((i) => i.label === menuItemLabel)).toBe(false);

    const finalHomeLayout = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/layouts/home`, { headers })).json()) as HomeLayoutResponse;
    expect(finalHomeLayout.publishedBlocks.map(blockToInput)).toEqual(homeLayoutBefore.publishedBlocks.map(blockToInput));

    // ---- 10. Temizlik: artık hiçbir düzen bloğu referans vermiyor - slider silinebilir ----
    const deleteSliderResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/sliders/${createdSliderId}`, { headers });
    expect(deleteSliderResponse.status()).toBe(204);
    createdSliderId = undefined; // `afterAll` güvenlik ağının zaten silinmiş slider'ı tekrar silmeye çalışmaması için.

    if (createdMediaId) {
      const deleteMediaResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/media/${createdMediaId}`, { headers });
      expect(deleteMediaResponse.status()).toBe(204);
      createdMediaId = undefined;
    }
  });
});
