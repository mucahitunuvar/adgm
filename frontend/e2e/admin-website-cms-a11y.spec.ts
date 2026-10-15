import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

const ADMIN_URL = 'http://localhost:5173/admin/';
const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
// admin-website.spec.ts'in kurduğu, bul-ya-da-oluştur deseniyle yönetilen sabit anahtarlı içerik tipi -
// "İçerikler"/"Medya kütüphanesi" ekranlarını taramak için yalnız listelerin dolu olması gerekiyor,
// hangi içerik tipinden geldiği önemsiz.

// Dilim 2a-3 kapanışı (plan madde 2): yeni CMS ekranlarının (menüler, slider+slide, rakamlar, pop-up,
// ana sayfa düzeni, içerik düzeni sekmesi) ve içerik/medya ekranlarının açık/koyu temada VE 390/1440
// genişlikte erişilebilirlik ihlali içermediğini doğrular.
const VIEWPORTS = [
  { label: '390px (mobil)', width: 390, height: 844 },
  { label: '1440px (masaüstü)', width: 1440, height: 900 },
] as const;
const THEMES = ['light', 'dark'] as const;

async function loginForApiToken(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, { data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD } });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

/** DOM'u yeniden yüklemeden (K-07 - SPA içi durum korunur) her viewport × tema kombinasyonunda axe çalıştırır. */
async function scanScreen(page: Page, label: string) {
  for (const viewport of VIEWPORTS) {
    await page.setViewportSize({ width: viewport.width, height: viewport.height });
    for (const theme of THEMES) {
      await page.emulateMedia({ colorScheme: theme });
      const results = await new AxeBuilder({ page }).analyze();
      expect(results.violations, `${label} @ ${viewport.label}, ${theme}`).toEqual([]);
    }
  }
}

test.describe.configure({ mode: 'serial' });

test.describe('admin: CMS ekranları erişilebilirlik taraması (390/1440 x açık/koyu, gerçek backend)', () => {
  let headers: Record<string, string>;
  const runId = Date.now();
  const sliderKey = `e2e-a11y-slider-${runId}`;
  const impactMetricLabel = `E2E A11y Rakam ${runId}`;
  const popupTitle = `E2E A11y Pop-up ${runId}`;
  const layoutItemTitle = `E2E A11y İçerik ${runId}`;
  let sliderId: string | undefined;
  let impactMetricId: string | undefined;
  let popupId: string | undefined;
  let layoutContentItemId: string | undefined;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request);
    headers = { Authorization: `Bearer ${token}` };

    const createSliderResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/sliders`, {
      headers,
      data: { key: sliderKey, defaultLanguageName: 'E2E A11y Slider' },
    });
    expect(createSliderResponse.status()).toBe(201);
    sliderId = ((await createSliderResponse.json()) as { id: string }).id;

    const createMetricResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/impact-metrics`, {
      headers,
      data: {
        value: 42,
        iconKey: null,
        sortOrder: 0,
        defaultLanguageLabel: impactMetricLabel,
        defaultLanguageUnit: null,
        defaultLanguagePeriod: null,
        defaultLanguageSource: null,
      },
    });
    expect(createMetricResponse.status()).toBe(201);
    impactMetricId = ((await createMetricResponse.json()) as { id: string }).id;

    const createPopupResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/popups`, {
      headers,
      data: {
        displayMode: 'Modal',
        imageMediaId: null,
        link: null,
        targeting: { kind: 'AllPages', contentItemIds: null, paths: null },
        deviceTarget: 'All',
        publishAtUtc: null,
        unpublishAtUtc: null,
        delaySeconds: 3,
        frequency: 'EveryVisit',
        frequencyDays: null,
        dismissible: true,
        priority: 0,
        defaultLanguageTitle: popupTitle,
        defaultLanguageBody: 'E2E a11y gövde',
        defaultLanguageButtonLabel: null,
      },
    });
    expect(createPopupResponse.status()).toBe(201);
    popupId = ((await createPopupResponse.json()) as { id: string }).id;

    // "İçerik düzeni" sekmesi, yalnız `supportsBlockLayout: true` olan bir içerik tipinde görünür -
    // `e2e-sabit` (diğer spec'lerin paylaştığı fixture) bunu desteklemiyor, bu yüzden kendi kalıcı
    // (bul-ya-da-oluştur, `e2e-sabit` ile aynı desen) içerik tipi fixture'ı kullanılıyor.
    const typesResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/content-types`, { headers });
    const types = (await typesResponse.json()) as Array<{ id: string; key: string; isActive: boolean; rowVersion: string }>;
    let layoutTypeId: string;
    const existingType = types.find((t) => t.key === 'e2e-a11y-layout');
    if (existingType) {
      if (!existingType.isActive) {
        await request.post(`${BACKEND_URL}/api/v1/admin/website/content-types/${existingType.id}/activate`, {
          headers,
          data: { rowVersion: existingType.rowVersion },
        });
      }
      layoutTypeId = existingType.id;
    } else {
      const createTypeResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/content-types`, {
        headers,
        data: {
          key: 'e2e-a11y-layout',
          listTemplate: 'default',
          detailTemplate: 'default',
          sortMode: 'Manual',
          sortOrder: 0,
          supportsHierarchy: false,
          supportsCategories: false,
          supportsTags: false,
          supportsDetailImage: true,
          supportsGallery: false,
          supportsVideos: false,
          supportsAttachments: false,
          supportsEvent: false,
          supportsBlockLayout: true,
          supportsForm: false,
          supportsRelatedContent: false,
          hasDetailPage: true,
          hasListingPage: true,
          isSearchable: true,
          requiresReview: false,
          defaultLanguageName: 'E2E A11y Düzen',
          defaultLanguageRoutePrefix: 'e2e-a11y-layout',
          schemaKind: 'None',
          seo: {
            metaTitle: null,
            metaDescription: null,
            metaKeywords: null,
            ogTitle: null,
            ogDescription: null,
            ogImageMediaId: null,
            canonicalUrl: null,
            noIndex: false,
          },
        },
      });
      expect(createTypeResponse.status()).toBe(201);
      layoutTypeId = ((await createTypeResponse.json()) as { id: string }).id;
    }

    const createItemResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers,
      data: {
        contentTypeId: layoutTypeId,
        parentId: null,
        sortOrder: 0,
        isFeatured: false,
        coverImageMediaId: null,
        detailImageMediaId: null,
        defaultLanguageTitle: layoutItemTitle,
        defaultLanguageSlug: `e2e-a11y-icerik-${runId}`,
        defaultLanguageSummary: null,
        defaultLanguageBody: null,
        seo: {
          metaTitle: null,
          metaDescription: null,
          metaKeywords: null,
          ogTitle: null,
          ogDescription: null,
          ogImageMediaId: null,
          canonicalUrl: null,
          noIndex: false,
        },
      },
    });
    expect(createItemResponse.status()).toBe(201);
    layoutContentItemId = ((await createItemResponse.json()) as { id: string }).id;
  });

  test.afterAll(async ({ request }) => {
    // Slider/rakam/pop-up/içerik bu testin kendi fixture'ı - tamamen temizlenir. İçerik tipi
    // (`e2e-a11y-layout`) `e2e-sabit` ile aynı kalıcı bul-ya-da-oluştur fixture deseni (silme ucu yok,
    // yeniden kullanılır) - bilerek silinmedi.
    if (layoutContentItemId) {
      const detailResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${layoutContentItemId}`, { headers });
      if (detailResponse.ok()) {
        const detail = (await detailResponse.json()) as { rowVersion: string };
        await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${layoutContentItemId}`, { headers, data: { rowVersion: detail.rowVersion } });
        await request.delete(`${BACKEND_URL}/api/v1/admin/website/trash/${layoutContentItemId}`, { headers });
      }
    }
    if (popupId) await request.delete(`${BACKEND_URL}/api/v1/admin/website/popups/${popupId}`, { headers });
    if (impactMetricId) await request.delete(`${BACKEND_URL}/api/v1/admin/website/impact-metrics/${impactMetricId}`, { headers });
    if (sliderId) await request.delete(`${BACKEND_URL}/api/v1/admin/website/sliders/${sliderId}`, { headers });
  });

  test('CMS ekranları açık/koyu temada ve 390/1440 genişlikte erişilebilirlik ihlali içermiyor', async ({ page }) => {
    test.setTimeout(300_000);

    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    // Bu oturumda gözlenen bir yavaşlık: Vite'ın dev proxy'si (`/api/*` -> backend) bazı isteklerde
    // olağan dışı yavaş (manuel ölçüm: tek bir `/auth/login` isteği 8.6 sn) - uzun bir koşum
    // geçmişinin (bu oturumda saatlerce çalışan dev sunucular) biriktirdiği bir kaynak baskısı
    // gibi görünüyor, kod regresyonu değil (doğrudan backend'e curl her zaman anında yanıt verdi).
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`), { timeout: 45_000 });

    // --- Menüler ---
    await page.getByRole('link', { name: 'Menüler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/menuler$/);
    await scanScreen(page, 'Menüler');

    // --- Slider'lar (liste) ---
    await page.getByRole('link', { name: "Slider'lar" }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/slider$/);
    await scanScreen(page, "Slider'lar (liste)");

    // --- Slider + slide düzenleme (detay + "Slide ekle" diyaloğu açık) ---
    await page.getByRole('row', { name: new RegExp(sliderKey) }).getByRole('link', { name: 'Görüntüle' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/slider\/[0-9a-fA-F-]{36}$/);
    await scanScreen(page, 'Slider detayı');
    await page.getByRole('button', { name: 'Slide ekle' }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await scanScreen(page, 'Slide düzenleme diyaloğu');
    await page.getByRole('button', { name: 'Vazgeç' }).click();

    // --- Rakamlar şeridi (liste + düzenleme diyaloğu) ---
    await page.getByRole('link', { name: 'Rakamlar şeridi' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/rakamlar$/);
    await scanScreen(page, 'Rakamlar şeridi (liste)');
    await page.getByRole('row', { name: new RegExp(impactMetricLabel) }).getByRole('button', { name: 'Düzenle' }).click();
    await scanScreen(page, 'Rakam düzenleme diyaloğu');
    await page.keyboard.press('Escape');

    // --- Pop-up / Banner (liste + detay) ---
    await page.getByRole('link', { name: 'Pop-up / Banner' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/popup$/);
    await scanScreen(page, 'Pop-up/Banner (liste)');
    await page.getByRole('row', { name: new RegExp(popupTitle) }).getByRole('link', { name: 'Görüntüle' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/popup\/[0-9a-fA-F-]{36}$/);
    await scanScreen(page, 'Pop-up detayı');

    // --- Ana sayfa düzeni ---
    await page.getByRole('link', { name: 'Ana sayfa düzeni' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/ana-sayfa-duzeni$/);
    await scanScreen(page, 'Ana sayfa düzeni');

    // --- İçerikler (liste) ---
    await page.getByRole('link', { name: 'İçerikler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler$/);
    await scanScreen(page, 'İçerikler (liste)');

    // --- İçerik düzeni sekmesi ---
    await page.getByLabel('Ara').fill(layoutItemTitle);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await page.getByRole('link', { name: 'Görüntüle' }).first().click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler\/[0-9a-fA-F-]{36}$/);
    await page.getByRole('tab', { name: 'Düzen' }).click();
    await scanScreen(page, 'İçerik düzeni sekmesi');

    // --- Medya kütüphanesi ---
    await page.getByRole('link', { name: 'Medya kütüphanesi' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/medya$/);
    await scanScreen(page, 'Medya kütüphanesi');
  });
});
