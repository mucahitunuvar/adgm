import { test, expect, type APIRequestContext } from '@playwright/test';

const ADMIN_URL = 'http://localhost:5173/admin/';
const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
// admin-website.spec.ts'in bul-ya-da-oluştur deseniyle yönetilen sabit anahtarlı içerik tipi -
// kategori desteği var (bu testin categoryId filtresi için yeterli).
const CONTENT_TYPE_KEY = 'e2e-sabit';

async function loginForApiToken(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, { data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD } });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

async function ensureContentTypeActive(request: APIRequestContext, headers: Record<string, string>): Promise<string> {
  const listResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/content-types`, { headers });
  const types = (await listResponse.json()) as Array<{ id: string; key: string; isActive: boolean; rowVersion: string }>;
  const type = types.find((t) => t.key === CONTENT_TYPE_KEY);
  if (!type) throw new Error(`İçerik tipi bulunamadı: ${CONTENT_TYPE_KEY}. admin-website.spec.ts önce çalıştırılmış olmalı.`);
  if (!type.isActive) {
    await request.post(`${BACKEND_URL}/api/v1/admin/website/content-types/${type.id}/activate`, {
      headers,
      data: { rowVersion: type.rowVersion },
    });
  }
  return type.id;
}

interface CreatedItem {
  id: string;
  title: string;
}

async function createItem(
  request: APIRequestContext, headers: Record<string, string>, contentTypeId: string, title: string, slug: string,
): Promise<CreatedItem> {
  const response = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
    headers,
    data: {
      contentTypeId,
      parentId: null,
      sortOrder: 0,
      isFeatured: false,
      coverImageMediaId: null,
      detailImageMediaId: null,
      defaultLanguageTitle: title,
      defaultLanguageSlug: slug,
      defaultLanguageSummary: null,
      defaultLanguageBody: null,
      seo: { metaTitle: null, metaDescription: null, metaKeywords: null, ogTitle: null, ogDescription: null, ogImageMediaId: null, canonicalUrl: null, noIndex: false },
    },
  });
  expect(response.status()).toBe(201);
  const body = (await response.json()) as { id: string };
  return { id: body.id, title };
}

async function deleteItem(request: APIRequestContext, headers: Record<string, string>, id: string): Promise<void> {
  const detailResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${id}`, { headers });
  expect(detailResponse.ok()).toBe(true);
  const detail = (await detailResponse.json()) as { rowVersion: string };
  const deleteResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${id}`, { headers, data: { rowVersion: detail.rowVersion } });
  expect(deleteResponse.status()).toBe(204);
  const trashDeleteResponse = await request.delete(`${BACKEND_URL}/api/v1/admin/website/trash/${id}`, { headers });
  expect(trashDeleteResponse.status()).toBe(204);
}

test.describe.configure({ mode: 'serial' });

test.describe('admin: içerik listesi kategori/sıralama filtreleri ve paylaşılabilir URL (gerçek backend)', () => {
  let headers: Record<string, string>;
  let contentTypeId: string;
  let categoryId: string;
  const runId = Date.now();
  // Ortak önek (`search` backend'de düz bir `Contains`, ayrıştırılmış sözcük eşleşmesi değil) her iki
  // başlığın da BAŞINDA - "Alfa"/"Beta" ayracı aradan deseni bölmesin diye sona konuldu.
  const itemAlfaTitle = `E2E Filtre ${runId} Alfa`;
  const itemBetaTitle = `E2E Filtre ${runId} Beta`;
  let itemAlfa: CreatedItem;
  let itemBeta: CreatedItem;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request);
    headers = { Authorization: `Bearer ${token}` };
    contentTypeId = await ensureContentTypeActive(request, headers);

    const categoryResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/content-types/${contentTypeId}/categories`, {
      headers,
      data: {
        parentId: null,
        sortOrder: 0,
        defaultLanguageName: `E2E Filtre Kategori ${runId}`,
        defaultLanguageSlug: `e2e-filtre-kategori-${runId}`,
        seo: { metaTitle: null, metaDescription: null, metaKeywords: null, ogTitle: null, ogDescription: null, ogImageMediaId: null, canonicalUrl: null, noIndex: false },
      },
    });
    expect(categoryResponse.status()).toBe(201);
    categoryId = ((await categoryResponse.json()) as { id: string }).id;

    // "Alfa" alfabetik olarak önce, ama kategoriye atanan tek içerik - categoryId filtresinin
    // gerçekten narrow ettiğini (Beta'yı DIŞARIDA bıraktığını) göstermek için.
    itemAlfa = await createItem(request, headers, contentTypeId, itemAlfaTitle, `e2e-filtre-alfa-${runId}`);
    const alfaDetail = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${itemAlfa.id}`, { headers })).json()) as { rowVersion: string };
    const setCategoryResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/contents/${itemAlfa.id}/categories`, {
      headers,
      data: { rowVersion: alfaDetail.rowVersion, categoryIds: [categoryId] },
    });
    expect(setCategoryResponse.status()).toBe(204);

    itemBeta = await createItem(request, headers, contentTypeId, itemBetaTitle, `e2e-filtre-beta-${runId}`);
  });

  test.afterAll(async ({ request }) => {
    if (itemAlfa) await deleteItem(request, headers, itemAlfa.id);
    if (itemBeta) await deleteItem(request, headers, itemBeta.id);
    if (categoryId) await request.delete(`${BACKEND_URL}/api/v1/admin/website/content-types/${contentTypeId}/categories/${categoryId}`, { headers });
  });

  test('kategori filtresi URL\'e yazılır, sayfayı o URL ile açmak aynı filtrelenmiş sonucu üretir (paylaşılabilir bağlantı)', async ({ page }) => {
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`));

    await page.getByRole('link', { name: 'İçerikler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler$/);

    // Önce arama ile yalnız bu çalıştırmanın iki öğesine indir - başka koşuların/dev verisinin
    // listeye karışmasını önler.
    await page.getByLabel('Ara').fill(`E2E Filtre ${runId}`);
    await page.getByRole('combobox', { name: 'İçerik tipi' }).selectOption(contentTypeId);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await expect(page.getByText(itemAlfaTitle)).toBeVisible();
    await expect(page.getByText(itemBetaTitle)).toBeVisible();

    // Kategori filtresi: yalnız Alfa kalmalı, Beta kaybolmalı.
    await page.getByRole('combobox', { name: 'Kategori' }).selectOption(categoryId);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await expect(page).toHaveURL(new RegExp(`categoryId=${categoryId}`));
    await expect(page.getByText(itemAlfaTitle)).toBeVisible();
    await expect(page.getByText(itemBetaTitle)).not.toBeVisible();

    // Geri/ileri uyumluluk (SPA içi, aynı oturum): her filtre uygulaması kendi tarayıcı geçmişi
    // girdisini bırakmalı - "geri" kategori filtresinden ÖNCEKİ duruma (ikisi de görünür, URL'de
    // categoryId yok), "ileri" tekrar kategori filtresine dönmeli.
    await page.goBack();
    await expect(page).not.toHaveURL(/categoryId=/);
    await expect(page.getByText(itemAlfaTitle)).toBeVisible();
    await expect(page.getByText(itemBetaTitle)).toBeVisible();
    await expect(page.getByRole('combobox', { name: 'Kategori' })).toHaveValue('');

    await page.goForward();
    await expect(page).toHaveURL(new RegExp(`categoryId=${categoryId}`));
    await expect(page.getByText(itemAlfaTitle)).toBeVisible();
    await expect(page.getByText(itemBetaTitle)).not.toBeVisible();
    await expect(page.getByRole('combobox', { name: 'Kategori' })).toHaveValue(categoryId);

    // Paylaşılabilirlik - GERÇEK senaryo: biri bu URL'i oturumu olmayan bir sekmede/tarayıcıda açar.
    // `page.goto()` tam sayfa navigasyonu olduğu için K-07'nin bellek tabanlı oturumunu zaten siler
    // (F5 ile aynı) - burada ayrıca çerez temizlemeye gerek yok. `ProtectedRoute` oturumsuz isteği
    // `/giris`'e yönlendirir (sorgu dizesi dahil `from` state'inde taşınır - bkz. ProtectedRoute.tsx
    // düzeltmesi); giriş yapınca uygulama otomatik olarak AYNI filtrelenmiş URL'e geri dönmeli.
    const sharedUrl = page.url();
    await page.goto(sharedUrl);
    await expect(page).toHaveURL(/\/giris/);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`categoryId=${categoryId}`));
    await expect(page.getByRole('combobox', { name: 'Kategori' })).toHaveValue(categoryId);
    await expect(page.getByText(itemAlfaTitle)).toBeVisible();
    await expect(page.getByText(itemBetaTitle)).not.toBeVisible();
  });

  test('başlık sütun başlığına tıklamak sıralamayı değiştirir ve URL\'e yansır', async ({ page }) => {
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await page.getByRole('link', { name: 'İçerikler' }).click();
    await page.getByLabel('Ara').fill(`E2E Filtre ${runId}`);
    await page.getByRole('button', { name: 'Filtrele' }).click();

    const rows = page.locator('table tbody tr');
    await expect(rows).toHaveCount(2);

    await page.getByRole('button', { name: /^Başlık/ }).click();
    await expect(page).toHaveURL(/orderBy=Title/);
    await expect(page).toHaveURL(/direction=Asc/);
    await expect(rows.first()).toContainText(itemAlfaTitle);

    await page.getByRole('button', { name: /^Başlık/ }).click();
    await expect(page).toHaveURL(/direction=Desc/);
    await expect(rows.first()).toContainText(itemBetaTitle);
  });

  test('geçersiz orderBy/direction veya ters tarih aralığı sunucudan 400 döner', async ({ request }) => {
    const invalidOrderBy = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers,
      params: { orderBy: 'NotAField' },
    });
    expect(invalidOrderBy.status()).toBe(400);

    const invalidRange = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers,
      params: { createdFromUtc: '2026-01-02T00:00:00Z', createdToUtc: '2026-01-01T00:00:00Z' },
    });
    expect(invalidRange.status()).toBe(400);
  });
});
