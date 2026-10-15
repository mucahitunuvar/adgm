import AxeBuilder from '@axe-core/playwright';
import { test, expect, type Page } from '@playwright/test';

const ADMIN_URL = 'http://localhost:5173/admin/';
const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';

// A11 §6b: editörü/UI'ı hiç kullanmadan backend'e doğrudan göndererek sanitizer'ı sınar - token
// burada bilerek programatik olarak alınır (K-07 bellek modeli yalnız tarayıcı UI'ını kapsar;
// test betiğinin kendi API isteği UI akışının bir parçası değildir).
async function loginForApiToken(request: Page['request']): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, {
    data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
  });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

async function setTheme(page: Page, theme: 'light' | 'dark') {
  await page.emulateMedia({ colorScheme: theme });
}

async function expectNoAxeViolations(page: Page) {
  const results = await new AxeBuilder({ page }).analyze();
  expect(results.violations).toEqual([]);
}

test.describe.configure({ mode: 'serial' });

test.describe('admin: zengin metin editörü (A11, Tiptap) - gerçek backend', () => {
  test('backende doğrudan gönderilen XSS yükleri sanitize edilir, editör hiç devrede değil', async ({ request }) => {
    const token = await loginForApiToken(request);
    const headers = { Authorization: `Bearer ${token}` };

    const typesResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/content-types`, { headers });
    const types = (await typesResponse.json()) as Array<{ id: string }>;
    const contentTypeId = types[0]!.id;

    const payloads = [
      '<p>Metin</p><script>alert(1)</script>',
      '<img src="x" onerror="alert(1)">',
      '<p><a href="javascript:alert(1)">tıkla</a></p>',
      '<p style="background:url(javascript:alert(1))">Stil</p>',
      '<iframe srcdoc="<script>alert(1)</script>"></iframe>',
      '<svg onload="alert(1)"><circle /></svg>',
    ].join('');

    const createResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers,
      data: {
        contentTypeId,
        parentId: null,
        sortOrder: 0,
        isFeatured: false,
        coverImageMediaId: null,
        detailImageMediaId: null,
        defaultLanguageTitle: `E2E XSS ${Date.now()}`,
        defaultLanguageSlug: null,
        defaultLanguageSummary: null,
        defaultLanguageBody: payloads,
        seo: {
          metaTitle: null, metaDescription: null, metaKeywords: null, ogTitle: null, ogDescription: null,
          ogImageMediaId: null, canonicalUrl: null, noIndex: false,
        },
      },
    });
    expect(createResponse.status()).toBe(201);
    const created = (await createResponse.json()) as { id: string };

    const detailResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${created.id}`, { headers });
    const detail = (await detailResponse.json()) as { rowVersion: string; translations: Array<{ body: string }> };
    const storedBody = detail.translations[0]!.body;

    for (const dangerous of ['<script', 'onerror', 'javascript:', 'style=', '<iframe', '<svg', 'onload']) {
      expect(storedBody).not.toContain(dangerous);
    }

    // Test verisini temizle - içerik tiplerinin aksine içeriklerin gerçek bir silme ucu var.
    await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${created.id}`, {
      headers,
      data: { rowVersion: detail.rowVersion },
    });
  });

  test('editörle yazılan içerik kaydedilir; editörün gönderdiği HTML ile backendin döndürdüğü arasındaki fark raporlanır', async ({ page }) => {
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`));

    let sentBody: string | undefined;
    await page.route('**/api/v1/admin/website/contents/*/translations/*', async (route) => {
      if (route.request().method() === 'PUT') {
        const payload = JSON.parse(route.request().postData() ?? '{}') as { body?: string };
        sentBody = payload.body;
      }
      await route.continue();
    });

    await page.getByRole('link', { name: 'İçerikler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler$/);
    await page.getByRole('button', { name: 'Yeni içerik' }).click();

    const title = `E2E İçerik ${Date.now()}`;
    await page.getByLabel('İçerik tipi').selectOption({ index: 1 });
    await page.getByLabel('Başlık (varsayılan dil)').fill(title);
    await page.getByRole('button', { name: 'Oluştur' }).click();

    await expect(page).toHaveURL(/\/site-yonetimi\/icerikler\/[0-9a-fA-F-]{36}$/);
    // Rota React.lazy ile ayrı bir parça (chunk) olarak yüklenir (A11 §1); axe taramasından önce
    // Suspense yer tutucusunun (h1 içermez) kapanıp gerçek sayfa başlığının göründüğünden emin ol.
    await expect(page.getByRole('heading', { level: 1, name: title })).toBeVisible();
    await expectNoAxeViolations(page);

    // --- Araç çubuğunu kullanarak biçimlendirilmiş içerik yaz. ---
    const body = page.getByRole('textbox', { name: 'İçerik gövdesi' });
    await body.click();
    await page.keyboard.type('Kalın metin');
    await page.keyboard.press('Control+A');
    await page.getByRole('button', { name: 'Kalın' }).click();
    await page.keyboard.press('End');
    await page.keyboard.press('Enter');
    await page.getByRole('button', { name: 'Başlık 2' }).click();
    await page.keyboard.type('Bir başlık');

    await page.getByRole('button', { name: 'Kaydet' }).click();
    await expect(page.getByText('İçerik kaydedildi.').last()).toBeVisible();

    expect(sentBody).toBeDefined();
    expect(sentBody).toContain('<strong>');
    expect(sentBody).toContain('<h2>');

    // Sayfayı yeniden yükleyip backend'in döndürdüğü (sanitize edilmiş) gövdeyi oku - K-07
    // nedeniyle oturum biter, bu yüzden yeniden giriş yapılır.
    await page.reload();
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();

    const reloadedBody = page.getByRole('textbox', { name: 'İçerik gövdesi' });
    await expect(reloadedBody).toBeVisible();
    await expect(reloadedBody.getByText('Kalın metin')).toBeVisible();
    await expect(reloadedBody.getByRole('heading', { name: 'Bir başlık' })).toBeVisible();

    for (const theme of ['light', 'dark'] as const) {
      await setTheme(page, theme);
      await expectNoAxeViolations(page);
    }
    await setTheme(page, 'light');
  });
});
