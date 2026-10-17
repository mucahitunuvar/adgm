import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

// Faz 2b Dilim 2b-2 (FRONTEND-FAZ2B-MASTER-PROMPT.md): form tanımları (alan yönetimi dahil), bir
// içeriğe bağlama, apps/web'de gerçek gönderim, form başvuruları yönetimi (durum/atama/not/dosya/
// arşiv). `mode: 'serial'` - 2b-1'deki diğer dosyalarla aynı nedenle (paylaşılan backend durumu).
const ADMIN_URL = 'http://localhost:5173/admin/';
const WEB_URL = 'http://localhost:3000';
const BACKEND_URL = 'http://localhost:5016';
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
const RUN_ID = Date.now();

// 1x1 PNG - `admin-website-layout-publish-chain.spec.ts`'in medya yükleme testiyle aynı sabit
// (gerçek dosya imzası kontrolünü geçer, FileSignatureValidator).
const PNG_1X1_BASE64 = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=';

const VIEWPORTS = [
  { label: '390px (mobil)', width: 390, height: 844 },
  { label: '1440px (masaüstü)', width: 1440, height: 900 },
] as const;
const THEMES = ['light', 'dark'] as const;

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

async function loginForApiToken(request: APIRequestContext, email: string, password: string): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, { data: { email, password } });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

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

test.describe('admin: formlar ve başvurular (Faz 2b Dilim 2b-2) - gerçek backend', () => {
  let adminHeaders: Record<string, string>;
  let contentItemId: string | undefined;
  const formKey = `e2e-form-${RUN_ID}`;
  const contentSlug = `e2e-form-sayfasi-2b2-${RUN_ID}`;
  const hostileText = `<script>alert(1)</script><img src=x onerror="alert(1)"> javascript:alert(1) ${'a'.repeat(300)}`;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request, ADMIN_EMAIL, ADMIN_PASSWORD);
    adminHeaders = authHeaders(token);
  });

  test.afterAll(async ({ request }) => {
    // Form tanımı/içerik GÜVENLİ şekilde tamamen silinebilir (başvurusu kalmadıktan sonra) - 2b-1'in
    // yasal metin kısıtlamasının aksine burada kalıcı bir backend invariant'ı yok.
    if (contentItemId) {
      const contentDetail = (await (
        await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers: adminHeaders })
      ).json()) as { rowVersion: string };
      await request.delete(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, {
        headers: adminHeaders,
        data: { rowVersion: contentDetail.rowVersion },
      });
    }
    // Form tanımı yalnız ÜZERİNDE başvuru yoksa silinebilir - bu testin kendi başvurusu kalıcı olarak
    // dursun diye (form başvuruları için backend'de bir silme ucu yok, ADR-024 §12.2 - durum/arşiv/
    // anonimleştirme ile yönetilir) formun KENDİSİ silinmez; e2e-sabit desenine düşer.
  });

  test('form tanımı + alanlar → içeriğe bağla → public gönderim → admin yönetimi → axe', async ({ page, request }) => {
    test.setTimeout(300_000);

    // ================= 1) Form tanımı oluştur =================
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`), { timeout: 45_000 });

    await page.getByRole('link', { name: 'Formlar' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/formlar$/);
    await scanScreen(page, 'Formlar (liste)');

    await page.getByRole('button', { name: 'Yeni form' }).click();
    const createDialog = page.getByRole('dialog');
    await createDialog.getByLabel('Anahtar').fill(formKey);
    await createDialog.getByLabel(/^Başlık/).fill('E2E Form');
    await createDialog.getByLabel(/^Başarı mesajı/).fill('Başvurunuz alındı.');
    await createDialog.getByLabel(/^Gönder düğmesi metni/).fill('Gönder');
    // `privacyNoticeKey` backend'de zorunludur (CreateFormDefinitionCommandValidator.NotEmpty) - 2b-1'in
    // kararlı fixture belgesi kullanılır.
    await createDialog.getByLabel(/^Aydınlatma metni/).selectOption({ label: 'E2E Sabit Aydınlatma Metni (e2e-sabit-legal-document)' });
    await createDialog.getByRole('button', { name: 'Oluştur' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/formlar\/[0-9a-f-]+$/);
    await scanScreen(page, 'Form detayı (Genel)');

    // ================= 2) Alanlar: metin, seçim, dosya =================
    await page.getByRole('tab', { name: 'Alanlar' }).click();
    await scanScreen(page, 'Form detayı (Alanlar, boş)');

    await page.getByRole('button', { name: 'Alan ekle' }).click();
    let fieldDialog = page.getByRole('dialog');
    await fieldDialog.getByLabel('Alan anahtarı').fill('ad_soyad');
    await fieldDialog.getByLabel('Alan türü').selectOption({ label: 'Metin' });
    await fieldDialog.getByLabel('Etiket').fill('Ad Soyad');
    await fieldDialog.getByRole('button', { name: 'Tamam' }).click();

    await page.getByRole('button', { name: 'Alan ekle' }).click();
    fieldDialog = page.getByRole('dialog');
    await fieldDialog.getByLabel('Alan anahtarı').fill('konu');
    await fieldDialog.getByLabel('Alan türü').selectOption({ label: 'Seçim listesi' });
    await fieldDialog.getByLabel('Etiket').fill('Konu');
    // Backend invariant'ı: Select/MultiSelect alanları en az 2 seçeneğe sahip olmalı.
    await fieldDialog.getByRole('button', { name: 'Seçenek ekle' }).click();
    await fieldDialog.getByLabel('Seçenek anahtarı').first().fill('genel');
    await fieldDialog.getByLabel(/^Etiket \(tr\)/).first().fill('Genel');
    await fieldDialog.getByRole('button', { name: 'Seçenek ekle' }).click();
    await fieldDialog.getByLabel('Seçenek anahtarı').last().fill('diger');
    await fieldDialog.getByLabel(/^Etiket \(tr\)/).last().fill('Diğer');
    await fieldDialog.getByRole('button', { name: 'Tamam' }).click();

    await page.getByRole('button', { name: 'Alan ekle' }).click();
    fieldDialog = page.getByRole('dialog');
    await fieldDialog.getByLabel('Alan anahtarı').fill('dosya');
    await fieldDialog.getByLabel('Alan türü').selectOption({ label: 'Dosya' });
    await fieldDialog.getByLabel('Etiket').fill('Dosya');
    await fieldDialog.getByLabel('PNG görseli').check();
    await fieldDialog.getByLabel(/^En fazla dosya boyutu/).fill('5');
    await fieldDialog.getByRole('button', { name: 'Tamam' }).click();

    await page.getByRole('button', { name: 'Alan listesini kaydet' }).click();
    await expect(page.getByText('Alan listesi kaydedildi.').last()).toBeVisible();
    await scanScreen(page, 'Form detayı (Alanlar, dolu)');

    // ================= 3) Formu etkinleştir =================
    await page.getByRole('button', { name: 'Etkinleştir' }).click();
    await expect(page.getByText('Form etkinleştirildi.').last()).toBeVisible();

    // ================= 4) Bir içeriğe bağla (içerik "page" tipinden, supportsForm) =================
    const contentTypes = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/content-types`, { headers: adminHeaders })
    ).json()) as Array<{ id: string; key: string }>;
    const pageTypeId = contentTypes.find((t) => t.key === 'page')!.id;

    const createContentResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents`, {
      headers: adminHeaders,
      data: {
        contentTypeId: pageTypeId,
        parentId: null,
        sortOrder: 1,
        isFeatured: false,
        coverImageMediaId: null,
        detailImageMediaId: null,
        defaultLanguageTitle: `E2E Form Sayfası 2b-2 ${RUN_ID}`,
        defaultLanguageSlug: contentSlug,
        defaultLanguageSummary: null,
        defaultLanguageBody: '<p>Aşağıdaki formu doldurun.</p>',
        seo: { metaTitle: null, metaDescription: null, ogTitle: null, ogDescription: null, ogImageMediaId: null, canonicalUrl: null, noIndex: false },
      },
    });
    expect(createContentResponse.ok(), await createContentResponse.text()).toBeTruthy();
    contentItemId = ((await createContentResponse.json()) as { id: string }).id;

    // `page.goto` ile doğrudan bir yönetici rotasına atlamak TAM SAYFA yenilemesi yapar - admin
    // uygulamasının bellekte tuttuğu oturum jetonu (FRONTEND-GUVENLIK.md §2, kalıcı depoda YOK)
    // bu yenilemede kaybolur ve sayfa "oturum süresi doldu"ya düşer. Bu yüzden girişten sonra her
    // zaman uygulama İÇİ bağlantılara tıklanır (2b-1'in testinin de yaptığı gibi), asla `page.goto`
    // ile bir /site-yonetimi/... rotasına doğrudan gidilmez.
    await page.getByRole('link', { name: 'İçerikler' }).click();
    await page.getByLabel('Ara').fill(`E2E Form Sayfası 2b-2 ${RUN_ID}`);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await page.getByRole('row', { name: new RegExp(`E2E Form Sayfası 2b-2 ${RUN_ID}`) }).getByRole('link', { name: 'Görüntüle' }).click();
    await expect(page).toHaveURL(new RegExp(`/site-yonetimi/icerikler/${contentItemId}$`));
    await page.getByRole('tab', { name: 'Form' }).click();
    await scanScreen(page, 'İçerik detayı (Form sekmesi)');
    await page.getByLabel(/^Form \(/).selectOption({ label: `E2E Form (${formKey})` });
    await page.getByRole('button', { name: 'Kaydet' }).click();
    await expect(page.getByText('Bağlı form güncellendi.').last()).toBeVisible();

    const contentDetailBeforePublish = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}`, { headers: adminHeaders })
    ).json()) as { rowVersion: string };
    const publishResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/contents/${contentItemId}/publish`, {
      headers: adminHeaders,
      data: { rowVersion: contentDetailBeforePublish.rowVersion, publishAtUtc: null, unpublishAtUtc: null },
    });
    expect(publishResponse.status(), await publishResponse.text()).toBe(204);

    // ================= 5) apps/web: form göründü ve gönderildi (honeypot/min. süre ile) =================
    await page.goto(`${WEB_URL}/${contentSlug}`);
    await expect(page.getByRole('heading', { name: `E2E Form Sayfası 2b-2 ${RUN_ID}` })).toBeVisible();

    await page.getByLabel('Ad Soyad').fill(hostileText);
    await page.getByLabel('Konu').selectOption({ label: 'Genel' });
    await page.locator('input[type="file"]').setInputFiles({ name: 'e2e-fixture.png', mimeType: 'image/png', buffer: Buffer.from(PNG_1X1_BASE64, 'base64') });

    // IPublicSubmissionGuard.MinimumFillDuration (2b-1'in bülten testiyle aynı 3 sn) - sayfa
    // yüklendikten en az bu kadar sonra gönderilmeli, aksi halde backend reddeder.
    await page.waitForTimeout(3_200);

    let alertFired = false;
    page.on('dialog', (dialog) => {
      alertFired = true;
      void dialog.dismiss();
    });

    await page.getByRole('button', { name: 'Gönder' }).click();
    await expect(page.getByRole('status').getByText('Başvurunuz alındı.')).toBeVisible({ timeout: 15_000 });
    expect(alertFired, 'düşmanca girdi (<script>/onerror/javascript:) tarayıcıda ÇALIŞMAMALI').toBe(false);

    // ================= 6) Admin: başvuru listesinde görünür → detay =================
    // apps/web'e geçiş farklı bir origin/SPA'dır - admin'in bellekteki oturumu bu geçişte zaten
    // kaybolur; yeniden giriş yapılır, ardından yine yalnız uygulama içi tıklamalarla gezinilir.
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`), { timeout: 45_000 });
    await page.getByRole('link', { name: 'Form başvuruları' }).click();
    await scanScreen(page, 'Form başvuruları (liste)');
    await page.getByLabel(/^Form \(/).selectOption({ label: `E2E Form (${formKey})` });
    await page.getByRole('button', { name: 'Filtrele' }).click();
    const submissionRow = page.getByRole('row', { name: /e2e-form-/ });
    await expect(submissionRow).toBeVisible();
    await submissionRow.getByRole('link', { name: 'Görüntüle' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/form-basvurulari\/[0-9a-f-]+$/);
    await scanScreen(page, 'Form başvurusu detayı');

    // Düşmanca girdi admin'de METİN olarak görünür, HTML'e yorumlanmaz (hiçbir <script>/onerror
    // çalışmaz - üstteki `alertFired` kontrolü zaten gönderim anında bunu kanıtladı; burada ayrıca
    // admin DETAY ekranında da literal metnin göründüğünü, sayfa düzeninin bozulmadığını doğruluyoruz).
    await expect(page.locator('body')).toContainText('<script>alert(1)</script>');
    await expect(page.getByRole('heading', { name: /^Başvuru/ })).toBeVisible();

    // ================= 7) Durum değiştir (New → InReview → Approved → Completed) =================
    await page.getByRole('button', { name: 'İnceleniyor yap' }).click();
    await expect(page.getByText('Durum güncellendi.').last()).toBeVisible();
    await page.getByRole('button', { name: 'Onaylandı yap' }).click();
    await expect(page.getByText('Durum güncellendi.').last()).toBeVisible();
    await page.getByRole('button', { name: 'Tamamlandı yap' }).click();
    await expect(page.getByText('Durum güncellendi.').last()).toBeVisible();

    // ================= 8) Ata =================
    await page.getByRole('button', { name: 'Ata' }).click();
    const assignDialog = page.getByRole('dialog');
    await assignDialog.getByLabel('Atanan kullanıcı').selectOption({ label: `${ADMIN_EMAIL} (Admin)` });
    await scanScreen(page, 'Başvuruyu ata diyaloğu');
    await assignDialog.getByRole('button', { name: 'Ata' }).click();
    await expect(page.getByText('Atama güncellendi.').last()).toBeVisible();

    // ================= 9) Not ekle =================
    await page.getByLabel('Yeni not').fill(`e2e not ${RUN_ID}`);
    await page.getByRole('button', { name: 'Not ekle' }).click();
    await expect(page.getByText('Not eklendi.').last()).toBeVisible();
    await expect(page.getByText(`e2e not ${RUN_ID}`)).toBeVisible();

    // ================= 10) Dosya indir =================
    const downloadPromise = page.waitForEvent('download');
    await page.getByRole('button', { name: 'İndir' }).click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe('e2e-fixture.png');

    // ================= 11) Arşivle / arşivden çıkar =================
    await page.getByRole('button', { name: 'Arşivle' }).click();
    await expect(page.getByText('Başvuru arşivlendi.').last()).toBeVisible();
    await expect(page.getByText('Bu başvuru arşivlenmiş')).toBeVisible();
    await page.getByRole('button', { name: 'Arşivden çıkar' }).click();
    await expect(page.getByText('Başvuru arşivden çıkarıldı.').last()).toBeVisible();
  });
});
