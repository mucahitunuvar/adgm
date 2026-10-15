import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

const ADMIN_URL = 'http://localhost:5173/admin/';
const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
// admin-website.spec.ts'in zaten kurduğu, bul-ya-da-oluştur deseniyle yönetilen sabit anahtarlı
// içerik tipi - kategori desteği var, galeri/video/etkinlik/hiyerarşi yok (bu test onlara dokunmuyor).
const CONTENT_TYPE_KEY = 'e2e-sabit';

interface FeatureSettings {
  rowVersion: string;
}

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

async function setTheme(page: Page, theme: 'light' | 'dark') {
  await page.emulateMedia({ colorScheme: theme });
}

test.describe.configure({ mode: 'serial' });

test.describe('admin: içerik yaşam döngüsü, eşzamanlılık, revizyon (gerçek backend)', () => {
  let headers: Record<string, string>;
  let contentTypeId: string;
  let contentItemId: string;
  const slug = `e2e-lifecycle-${Date.now()}`;
  const originalTitle = `E2E Lifecycle ${Date.now()}`;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request);
    headers = { Authorization: `Bearer ${token}` };
    contentTypeId = await ensureContentTypeActive(request, headers);
  });

  test.afterAll(async ({ request }) => {
    // Temizlik: her koşulda (test başarısız olsa da) oluşturulan içerik kalıcı olarak silinir.
    if (!contentItemId) return;
    const detailResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers });
    if (!detailResponse.ok()) return;
    const detail = (await detailResponse.json()) as { rowVersion: string; status: string };
    if (detail.status === 'Published') {
      await request.post(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/unpublish`, {
        headers,
        data: { rowVersion: detail.rowVersion },
      });
    }
    const afterUnpublish = await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json() as { rowVersion: string };
    await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, {
      headers,
      data: { rowVersion: afterUnpublish.rowVersion },
    });
    await request.delete(`${BACKEND_URL}/api/v1/admin/website/trash/${contentItemId}`, { headers });
  });

  test('içerik oluşturulur, yayınlanır, public/contents ve routes/resolve\'da görünür, yayından kaldırılınca kaybolur', async ({ request }) => {
    const createResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers,
      data: {
        contentTypeId,
        parentId: null,
        sortOrder: 0,
        isFeatured: false,
        coverImageMediaId: null,
        detailImageMediaId: null,
        defaultLanguageTitle: originalTitle,
        defaultLanguageSlug: slug,
        defaultLanguageSummary: 'e2e özet',
        defaultLanguageBody: '<p>e2e gövde</p>',
        seo: { metaTitle: null, metaDescription: null, metaKeywords: null, ogTitle: null, ogDescription: null, ogImageMediaId: null, canonicalUrl: null, noIndex: false },
      },
    });
    expect(createResponse.status()).toBe(201);
    const created = (await createResponse.json()) as { id: string };
    contentItemId = created.id;

    // Yayından önce: public listede ve route çözümlemesinde görünmemeli.
    const beforePublishList = await request.get(`${BACKEND_URL}/api/v1/public/contents`, { params: { type: CONTENT_TYPE_KEY } });
    const beforePublishItems = (await beforePublishList.json()) as { items: { items: Array<{ id: string }> } };
    expect(beforePublishItems.items.items.some((i) => i.id === contentItemId)).toBe(false);

    const detailBefore = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as {
      rowVersion: string;
      translations: Array<{ languageCode: string; fullPath: string }>;
    };
    // Admin API `fullPath`'i başında `/` olmadan döner (ör. "e2e-sabit/slug"); public resolver
    // kanonik biçimle (başında `/`) birebir string eşitliği arar, aksi halde 301 Redirect döner
    // (bkz. RouteResolutionService.ResolveAsync, satır ~59-63) - "NotFound" yerine önce bunu alırız.
    const fullPath = `/${detailBefore.translations[0]!.fullPath}`;

    const resolveBefore = await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve`, { params: { path: fullPath } });
    const resolveBeforeBody = (await resolveBefore.json()) as { kind: string };
    expect(resolveBeforeBody.kind).toBe('NotFound');

    // Yayınla.
    const publishResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/publish`, {
      headers,
      data: { rowVersion: detailBefore.rowVersion, publishAtUtc: null, unpublishAtUtc: null },
    });
    expect(publishResponse.status()).toBe(204);

    const afterPublishList = await request.get(`${BACKEND_URL}/api/v1/public/contents`, { params: { type: CONTENT_TYPE_KEY } });
    const afterPublishItems = (await afterPublishList.json()) as { items: { items: Array<{ id: string }> } };
    expect(afterPublishItems.items.items.some((i) => i.id === contentItemId)).toBe(true);

    const resolveAfter = await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve`, { params: { path: fullPath } });
    const resolveAfterBody = (await resolveAfter.json()) as { kind: string; contentItemId: string };
    expect(resolveAfterBody.kind).toBe('Detail');
    expect(resolveAfterBody.contentItemId).toBe(contentItemId);

    // Yayından kaldır: ikisinden de kaybolmalı.
    const detailAfterPublish = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as FeatureSettings;
    const unpublishResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/unpublish`, {
      headers,
      data: { rowVersion: detailAfterPublish.rowVersion },
    });
    expect(unpublishResponse.status()).toBe(204);

    const afterUnpublishList = await request.get(`${BACKEND_URL}/api/v1/public/contents`, { params: { type: CONTENT_TYPE_KEY } });
    const afterUnpublishItems = (await afterUnpublishList.json()) as { items: { items: Array<{ id: string }> } };
    expect(afterUnpublishItems.items.items.some((i) => i.id === contentItemId)).toBe(false);

    const resolveAfterUnpublish = await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve`, { params: { path: fullPath } });
    const resolveAfterUnpublishBody = (await resolveAfterUnpublish.json()) as { kind: string };
    expect(resolveAfterUnpublishBody.kind).toBe('NotFound');
  });

  test('eski rowVersion ile güncelleme 409 ile düşer (eşzamanlılık)', async ({ request }) => {
    const detail = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as {
      rowVersion: string;
      translations: Array<{ languageCode: string; title: string; slug: string; summary: string; body: string; seo: unknown }>;
    };
    const translation = detail.translations[0]!;
    const staleRowVersion = detail.rowVersion;

    // Birinci "oturum": günceller, rowVersion ilerler.
    const firstUpdate = await request.put(
      `${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/translations/${translation.languageCode}`,
      { headers, data: { rowVersion: staleRowVersion, title: 'E2E Lifecycle - birinci güncelleme', slug: translation.slug, summary: translation.summary, body: translation.body, seo: translation.seo } },
    );
    expect(firstUpdate.status()).toBe(204);

    // İkinci "oturum" (eski rowVersion'ı hâlâ elinde tutan): aynı eski değerle tekrar yazmaya çalışır.
    const secondUpdate = await request.put(
      `${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/translations/${translation.languageCode}`,
      { headers, data: { rowVersion: staleRowVersion, title: 'E2E Lifecycle - ikinci (çakışan) güncelleme', slug: translation.slug, summary: translation.summary, body: translation.body, seo: translation.seo } },
    );
    expect(secondUpdate.status()).toBe(409);
  });

  test('revizyon geri yüklenince içerik önceki haline döner', async ({ request }) => {
    const detail = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as {
      rowVersion: string;
      translations: Array<{ languageCode: string; title: string; slug: string; summary: string; body: string; seo: unknown }>;
    };
    const translation = detail.translations[0]!;
    const revisionsBefore = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/revisions`, { headers, params: { page: 1, pageSize: 1 } })).json()) as {
      items: Array<{ revisionNumber: number }>;
    };
    const firstRevisionNumber = revisionsBefore.items.at(-1)!.revisionNumber;

    const titleBeforeEdit = translation.title;
    const editResponse = await request.put(
      `${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/translations/${translation.languageCode}`,
      { headers, data: { rowVersion: detail.rowVersion, title: 'E2E Lifecycle - geri yükleme öncesi düzenleme', slug: translation.slug, summary: translation.summary, body: translation.body, seo: translation.seo } },
    );
    expect(editResponse.status()).toBe(204);

    // `RestoreContentItemRevisionRequest.RowVersion` C#'ta nullable (`byte[]?`) ama validator
    // `NotEmpty()` ister (FluentValidation'ın net bir "eksik alan" mesajı verebilmesi için
    // nullable - boş geçilebilir anlamına gelmiyor); güncel rowVersion'ı göndermek gerekiyor.
    const detailAfterEdit = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as FeatureSettings;
    const restoreResponse = await request.post(
      `${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/revisions/${firstRevisionNumber}/restore`,
      { headers, data: { rowVersion: detailAfterEdit.rowVersion, languageCode: translation.languageCode, restoreCategories: true } },
    );
    expect(restoreResponse.status()).toBe(204);

    const detailAfterRestore = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers })).json()) as {
      translations: Array<{ languageCode: string; title: string }>;
    };
    const titleAfterRestore = detailAfterRestore.translations.find((t) => t.languageCode === translation.languageCode)!.title;
    expect(titleAfterRestore).toBe(titleBeforeEdit);
  });

  test('içerik detay sayfası açık ve koyu temada erişilebilirlik ihlali içermez', async ({ page }) => {
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`));

    // K-07 (bellek tabanlı oturum): ikinci bir `page.goto()` tam sayfa yenilemesi anlamına gelir ve
    // bellekteki access token'ı siler (tıpkı F5 gibi) - detay sayfasına her zaman SPA içi bağlantı
    // tıklamasıyla gidilir, doğrudan URL'e goto ile değil.
    await page.getByRole('link', { name: 'İçerikler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler$/);
    // Önceki testler başlığı birkaç kez değiştirdi (son hal tam olarak `originalTitle` olmayabilir -
    // revizyon listesi sıralaması test 3'te varsayımdan farklı çıkabilir); tüm varyantlarda ortak olan
    // sabit önekle arar.
    await page.getByLabel('Ara').fill('E2E Lifecycle');
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await page.getByRole('link', { name: 'Görüntüle' }).first().click();
    await expect(page.getByRole('tab', { name: 'Genel' })).toBeVisible();

    for (const theme of ['light', 'dark'] as const) {
      await setTheme(page, theme);
      const results = await new AxeBuilder({ page }).analyze();
      expect(results.violations).toEqual([]);
    }
  });
});
