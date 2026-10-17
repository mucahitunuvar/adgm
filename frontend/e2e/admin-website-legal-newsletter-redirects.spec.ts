import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

// Faz 2b Dilim 2b-1 (FRONTEND-FAZ2B-MASTER-PROMPT.md): yasal metinler (sürümlü akış), ayarlara
// bağlama (Newsletter/Events/CookieConsent için yasal metin seçicisi), bülten aboneleri, çerez özeti,
// yönlendirmeler ve 404 yolları. Bu dosyanın tüm testleri paylaşılan SiteSettings alanlarını
// (newsletterPrivacyNoticeKey/eventPrivacyNoticeKey) değiştirip `afterAll`'da geri alır - diğer
// dosyalarla aynı nedenle (bkz. web-content.spec.ts'in başlık yorumu) `mode: 'serial'` zorunlu.
const ADMIN_URL = 'http://localhost:5173/admin/';
const WEB_URL = 'http://localhost:3000';
const BACKEND_URL = 'http://localhost:5016';
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
const RUN_ID = Date.now();

// "Yasal metinler" ekranının kendisi bu çalıştırmada test edildiği için - `LegalDocument` hiç
// yayınlanmış sürümü olduktan sonra SİLİNEMEZ (backend invariant, `DeleteLegalDocumentCommandHandler`
// "HasEverBeenPublished" kontrolü doğrulandı) - sabit/bul-ya-da-oluştur anahtar kullanılır
// (`admin-website.spec.ts`'in "e2e-sabit" içerik tipi ile aynı desen); belge hiç silinmez, yalnız
// üstüne yeni sürümler eklenir.
const LEGAL_DOCUMENT_KEY = 'e2e-sabit-legal-document';

const VIEWPORTS = [
  { label: '390px (mobil)', width: 390, height: 844 },
  { label: '1440px (masaüstü)', width: 1440, height: 900 },
] as const;
const THEMES = ['light', 'dark'] as const;

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

async function loginForApiToken(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${BACKEND_URL}/api/v1/auth/login`, {
    data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
  });
  const body = (await response.json()) as { accessToken: string };
  return body.accessToken;
}

/** DOM'u yeniden yüklemeden (K-07) her viewport × tema kombinasyonunda axe çalıştırır. */
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

test.describe('admin: yasal metinler, bülten/çerez ayarları, bülten aboneleri, çerez özeti, yönlendirmeler (Faz 2b Dilim 2b-1) - gerçek backend', () => {
  let headers: Record<string, string>;
  let originalSettings: {
    rowVersion: string;
    newsletterPrivacyNoticeKey: string | null;
    eventPrivacyNoticeKey: string | null;
  };
  let legalDocumentId: string;
  let createdRedirectId: string | undefined;
  let createdNotFoundRedirectId: string | undefined;
  let notFoundLogId: string | undefined;
  let newsletterSubscriberId: string | undefined;
  const redirectFromPath = `e2e-redirect-${RUN_ID}`;
  const notFoundPath = `e2e-notfound-${RUN_ID}`;
  const subscriberEmail = `e2e-2b1-${RUN_ID}@example.com`;

  test.beforeAll(async ({ request }) => {
    const token = await loginForApiToken(request);
    headers = authHeaders(token);

    const settingsResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers });
    const settings = (await settingsResponse.json()) as {
      rowVersion: string;
      newsletterPrivacyNoticeKey: string | null;
      eventPrivacyNoticeKey: string | null;
    };
    originalSettings = {
      rowVersion: settings.rowVersion,
      newsletterPrivacyNoticeKey: settings.newsletterPrivacyNoticeKey,
      eventPrivacyNoticeKey: settings.eventPrivacyNoticeKey,
    };

    // Bul-ya-da-oluştur: belge bir önceki koşudan kalmış olabilir (hiç silinemez).
    const listResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents`, { headers });
    const list = (await listResponse.json()) as Array<{ id: string; key: string }>;
    const existing = list.find((d) => d.key === LEGAL_DOCUMENT_KEY);
    if (existing) {
      legalDocumentId = existing.id;
    } else {
      const createResponse = await request.post(`${BACKEND_URL}/api/v1/admin/website/legal-documents`, {
        headers,
        data: { key: LEGAL_DOCUMENT_KEY, kind: 'PrivacyNotice', defaultLanguageTitle: 'E2E Sabit Aydınlatma Metni' },
      });
      expect(createResponse.status()).toBe(201);
      legalDocumentId = ((await createResponse.json()) as { id: string }).id;
    }

    // Düşmanca girdi (admin-rich-text.spec.ts'teki ile aynı desen): editörü/UI'ı hiç kullanmadan
    // backend'e doğrudan gönderilen XSS yükleri sanitize ediliyor mu? Bilerek `beforeAll`'da, UI
    // testinden TAMAMEN ayrı bir taslak döngüsüyle çalışır (oluştur → yaz → doğrula → at) - aksi
    // halde bu isteğin ilerlettiği `rowVersion`, aşağıdaki UI testinin (sayfa yüklemesinde önbelleğe
    // aldığı) kendi `rowVersion`'ıyla çakışıp 409'a düşürürdü. Taslak sonda silindiği için UI testi
    // kendi "taslak varsa önce at" adımına hiç ihtiyaç duymadan temiz başlar.
    let beforeHostileDraft = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}`, { headers })
    ).json()) as { rowVersion: string; versions: Array<{ status: string }> };
    // Önceki bir koşu/deneme yarıda kesilip taslağı atmadan kalmış olabilir - "aynı anda tek taslak"
    // kuralı (`LegalDocument.DraftAlreadyExists`) buna takılır; önce temizlenir.
    if (beforeHostileDraft.versions.some((v) => v.status === 'Draft')) {
      const discardStaleDraftResponse = await request.delete(
        `${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}/versions/draft`,
        { headers, data: { rowVersion: beforeHostileDraft.rowVersion } },
      );
      expect(discardStaleDraftResponse.status()).toBe(204);
      beforeHostileDraft = (await (
        await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}`, { headers })
      ).json()) as { rowVersion: string; versions: Array<{ status: string }> };
    }
    const hostileDraftResponse = await request.post(
      `${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}/versions/draft`,
      { headers, data: { rowVersion: beforeHostileDraft.rowVersion, changeSummary: null } },
    );
    expect(hostileDraftResponse.status()).toBe(201);

    const beforeHostileWrite = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}`, { headers })
    ).json()) as { rowVersion: string };
    const hostileBody = '<p>Metin</p><script>alert(1)</script><img src="x" onerror="alert(1)"><p><a href="javascript:alert(1)">tıkla</a></p>';
    const hostileWriteResponse = await request.put(
      `${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}/versions/draft/translations/tr`,
      { headers, data: { rowVersion: beforeHostileWrite.rowVersion, body: hostileBody } },
    );
    expect(hostileWriteResponse.status()).toBe(204);

    const afterHostileWrite = (await (
      await request.get(`${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}`, { headers })
    ).json()) as { rowVersion: string; versions: Array<{ status: string; translations: Array<{ languageCode: string; body: string }> }> };
    const hostileStoredBody = afterHostileWrite.versions.find((v) => v.status === 'Draft')!.translations.find((t) => t.languageCode === 'tr')!.body;
    for (const dangerous of ['<script', 'onerror', 'javascript:']) {
      expect(hostileStoredBody).not.toContain(dangerous);
    }

    const discardHostileDraftResponse = await request.delete(
      `${BACKEND_URL}/api/v1/admin/website/legal-documents/${legalDocumentId}/versions/draft`,
      { headers, data: { rowVersion: afterHostileWrite.rowVersion } },
    );
    expect(discardHostileDraftResponse.status()).toBe(204);
  });

  test.afterAll(async ({ request }) => {
    // SiteSettings'i orijinal haline geri al - test ortamı kalıcı olarak değiştirilmez.
    async function currentRowVersion(): Promise<string> {
      const response = await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers });
      return ((await response.json()) as { rowVersion: string }).rowVersion;
    }
    await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/newsletter`, {
      headers,
      data: { rowVersion: await currentRowVersion(), newsletterPrivacyNoticeKey: originalSettings.newsletterPrivacyNoticeKey },
    });
    await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/events`, {
      headers,
      data: { rowVersion: await currentRowVersion(), eventPrivacyNoticeKey: originalSettings.eventPrivacyNoticeKey },
    });

    // Tamamen temizlenebilenler: abone, yönlendirmeler, 404 kaydı. Yasal metin BİLEREK silinmiyor
    // (yukarıdaki not).
    if (newsletterSubscriberId) {
      await request.delete(`${BACKEND_URL}/api/v1/admin/website/newsletter-subscribers/${newsletterSubscriberId}`, { headers });
    }
    if (createdRedirectId) {
      await request.delete(`${BACKEND_URL}/api/v1/admin/website/redirects/${createdRedirectId}`, { headers });
    }
    if (createdNotFoundRedirectId) {
      await request.delete(`${BACKEND_URL}/api/v1/admin/website/redirects/${createdNotFoundRedirectId}`, { headers });
    }
    if (notFoundLogId) {
      await request.delete(`${BACKEND_URL}/api/v1/admin/website/not-found-paths/${notFoundLogId}`, { headers });
    }
  });

  test('yasal metin taslak→yayın akışı; ayarlara bağlama; bülten aboneliği; yönlendirmeler; 404→dönüştürme; axe', async ({ page, request }) => {
    test.setTimeout(300_000);

    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`), { timeout: 45_000 });

    // ================= 1) Yasal metinler: liste, taslak oluştur/düzenle, yayınla =================
    await page.getByRole('link', { name: 'Yasal metinler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/yasal-metinler$/);
    await scanScreen(page, 'Yasal metinler (liste)');

    await page.getByRole('row', { name: new RegExp(LEGAL_DOCUMENT_KEY) }).getByRole('button', { name: 'Aç' }).click();
    await expect(page).toHaveURL(new RegExp(`/site-yonetimi/yasal-metinler/${legalDocumentId}$`));
    await scanScreen(page, 'Yasal metin detayı');

    // Zaten bir taslak varsa (önceki çalıştırma yarıda kesilmişse) önce onu at - deterministik başlangıç.
    if (await page.getByRole('button', { name: 'Taslağı sil' }).isVisible().catch(() => false)) {
      await page.getByRole('button', { name: 'Taslağı sil' }).click();
      await page.getByRole('button', { name: 'Onayla' }).click();
      await expect(page.getByText('Taslak silindi.').last()).toBeVisible();
    }

    await page.getByRole('button', { name: 'Yeni taslak oluştur' }).click();
    await expect(page.getByText('Taslak oluşturuldu.').last()).toBeVisible();

    const bodyEditor = page.getByRole('textbox', { name: 'Yasal metin gövdesi' });
    await bodyEditor.click();
    await page.keyboard.type(`E2E yasal metin gövdesi ${RUN_ID}.`);
    await page.getByRole('button', { name: 'Taslağı kaydet' }).click();
    await expect(page.getByText('Taslak gövde kaydedildi.').last()).toBeVisible();

    await page.getByRole('button', { name: 'Önizle' }).click();
    await expect(page.locator('.legal-document-preview').getByText(`E2E yasal metin gövdesi ${RUN_ID}.`)).toBeVisible();

    await page.getByRole('button', { name: 'Yayınla' }).click();
    const publishDialog = page.getByRole('dialog');
    await expect(publishDialog).toBeVisible();
    await scanScreen(page, 'Taslağı yayınla diyaloğu');
    await publishDialog.getByRole('button', { name: 'Yayınla' }).click();
    await expect(page.getByText('Yeni sürüm yürürlüğe girdi.').last()).toBeVisible();
    await expect(page.getByText(/^Yürürlükte: v/)).toBeVisible();

    // ================= 2) Ayarlara bağlama: Newsletter seçicisi, serbest metin değil =================
    await page.getByRole('link', { name: 'Site ayarları' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/ayarlar$/);
    await page.getByRole('tab', { name: 'Bülten' }).click();
    await scanScreen(page, 'Ayarlar - Bülten sekmesi');

    // Yayınlı sürümü olmayan bir anahtar seçilemez (seçenek listesinde hiç yok) - var olan,
    // yayınlı sürümlü belgeyi seçiyoruz.
    await page.getByLabel('Gizlilik bildirimi').selectOption({ label: `E2E Sabit Aydınlatma Metni (${LEGAL_DOCUMENT_KEY})` });
    await expect(page.getByText(/^Yürürlükteki sürüm: v/)).toBeVisible();
    await page.getByRole('button', { name: 'Kaydet' }).click();
    await expect(page.getByText('Bülten ayarları güncellendi.').last()).toBeVisible();

    // GET /public/site anında yansır (ADR-024 §17 invalidation).
    const publicSiteResponse = await request.get(`${BACKEND_URL}/api/v1/public/site`);
    const publicSite = (await publicSiteResponse.json()) as {
      newsletterPrivacyNotice: { key: string; version: number } | null;
    };
    expect(publicSite.newsletterPrivacyNotice?.key).toBe(LEGAL_DOCUMENT_KEY);
    const acceptedVersion = publicSite.newsletterPrivacyNotice!.version;

    // apps/web (Faz 3, NewsletterSection) `GET /public/site`'ı kendi ~60 sn'lik Next.js sunucu
    // `fetch` önbelleğiyle okur (`getSite.ts`) - bu, backend'in ANINDA invalide ettiği önbellekten
    // (yukarıdaki istek) AYRI ve kasıtlı bir katman (K-05/FRONTEND-MIMARI.md §3); bu yüzden
    // "etkin oldu" burada 60 sn beklemeden sınanamaz - bu geçiş zaten web-content.spec.ts'in kendi
    // fixture akışında kanıtlanmış (ADR-024 §14 madde 36). Burada yalnız sayfanın bu 2b-1 oturumunda
    // değiştirilen diğer dosyalardan (apps/web query-param tip düzeltmesi) dolayı kırılmadığını
    // doğruluyoruz.
    const webHomeResponse = await request.get(WEB_URL);
    expect(webHomeResponse.status()).toBe(200);
    expect(await webHomeResponse.text()).toContain('newsletter-section');

    // ================= 3) Bülten: public'ten abone ol (gönderim jetonu + min. doldurma süresi) =================
    const tokenResponse = await request.get(`${BACKEND_URL}/api/v1/public/submission-tokens`);
    const { token: submissionToken } = (await tokenResponse.json()) as { token: string };
    await new Promise((resolve) => setTimeout(resolve, 3_100)); // IPublicSubmissionGuard.MinimumFillDuration (3 sn)
    const subscribeResponse = await request.post(`${BACKEND_URL}/api/v1/public/newsletter/subscriptions`, {
      data: {
        submissionToken,
        turnstileToken: null,
        website: null,
        email: subscriberEmail,
        lang: 'tr',
        acceptedPrivacyNoticeVersion: acceptedVersion,
      },
    });
    expect(subscribeResponse.status(), await subscribeResponse.text()).toBe(202);

    // ================= 4) Bülten aboneleri ekranı: görünür, CSV indirilebilir, silinebilir =================
    await page.getByRole('link', { name: 'Bülten aboneleri' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/bulten-aboneleri$/);
    await page.getByLabel('E-posta ara').fill(subscriberEmail);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    const subscriberRow = page.getByRole('row', { name: new RegExp(subscriberEmail) });
    await expect(subscriberRow).toBeVisible();
    await scanScreen(page, 'Bülten aboneleri (liste)');

    // URL'ye PII sızmadığını doğrula (FRONTEND-GUVENLIK.md §5 / Dilim 2b-1 PII kuralları).
    expect(page.url()).not.toContain(encodeURIComponent(subscriberEmail));
    expect(page.url()).not.toContain('@');

    const downloadPromise = page.waitForEvent('download');
    await page.getByRole('button', { name: 'CSV olarak dışa aktar' }).click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toMatch(/\.csv$/);
    await expect(page.getByText('Dışa aktarma indirildi; erişim kaydına yazıldı.').last()).toBeVisible();

    await subscriberRow.getByRole('button', { name: 'Sil' }).click();
    await page.getByRole('button', { name: 'Onayla' }).click();
    await expect(page.getByText('Abone silindi.').last()).toBeVisible();
    await expect(subscriberRow).toHaveCount(0);
    newsletterSubscriberId = undefined; // UI üzerinden silindi; afterAll'un tekrar silmeye çalışmasına gerek yok.

    // ================= 5) Çerez özeti ekranı: toplulaştırılmış sayılar, kişi bazlı veri yok =================
    await page.getByRole('link', { name: 'Çerez özeti' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/cerez-ozeti$/);
    await expect(page.getByText('Toplam kayıt:')).toBeVisible();
    await scanScreen(page, 'Çerez özeti');

    // ================= 6) Yönlendirmeler: geçersiz hedef admin'de reddedilir; geçerli hedef public'te çalışır =================
    await page.getByRole('link', { name: 'Yönlendirmeler' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/yonlendirmeler$/);
    await scanScreen(page, 'Yönlendirmeler sekmesi');

    await page.getByRole('button', { name: 'Yeni yönlendirme' }).click();
    const redirectDialog = page.getByRole('dialog');
    await redirectDialog.getByLabel('Kaynak yol').fill(redirectFromPath);
    await redirectDialog.getByLabel('Hedef yol').fill('//evil.example.com');
    await expect(redirectDialog.getByText(/^Geçersiz yol/)).toBeVisible();
    await expect(redirectDialog.getByRole('button', { name: 'Kaydet' })).toBeDisabled();

    await redirectDialog.getByLabel('Hedef yol').fill(`e2e-redirect-target-${RUN_ID}`);
    await expect(redirectDialog.getByRole('button', { name: 'Kaydet' })).toBeEnabled();
    await redirectDialog.getByRole('button', { name: 'Kaydet' }).click();
    await expect(page.getByText('Yönlendirme oluşturuldu.').last()).toBeVisible();

    const redirectRow = page.getByRole('row', { name: new RegExp(redirectFromPath) });
    await expect(redirectRow).toBeVisible();
    // Satırda kimlik görünmez; temizlik için API'den bulunur.
    const redirectsListResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/redirects?search=${redirectFromPath}`, { headers });
    const redirectsList = (await redirectsListResponse.json()) as { items: Array<{ id: string; fromPath: string }> };
    createdRedirectId = redirectsList.items.find((r) => r.fromPath === redirectFromPath)?.id;
    expect(createdRedirectId).toBeDefined();

    // Public route resolve üzerinden gerçekten çalıştığını doğrula.
    const resolveResponse = await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve?path=%2F${redirectFromPath}`);
    const resolved = (await resolveResponse.json()) as { kind: string; location: string | null };
    expect(resolved.kind).toBe('Redirect');
    expect(resolved.location).toBe(`/e2e-redirect-target-${RUN_ID}`);

    // ================= 7) 404 yolu üretilir, admin'de yönlendirmeye çevrilir =================
    // Paylaşılan dev veritabanı, önceki koşulardan kalma (backend'in kendi 90 gün/sayaç<5 temizlik
    // job'ına bırakılmış, bilerek silinmeyen) onlarca `NotFoundLog` kaydı taşıyor - liste `hitCount`
    // DESC sıralı ve sayfa başına en çok 100 kayıt döner; tek bir çözümleme isteği (hitCount=1) listenin
    // dibine düşüp hiçbir sayfada deterministik olarak bulunamayabilir. Aynı yola art arda birkaç kez
    // istek atmak, mevcut en yüksek sayaçtan (gözlemlendi: 31) kesin olarak yukarı çıkarır - ilk sayfada
    // görünmesi garanti olur.
    for (let i = 0; i < 35; i += 1) {
      await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve?path=%2F${notFoundPath}`);
    }
    const notFoundResolveResponse = await request.get(`${BACKEND_URL}/api/v1/public/routes/resolve?path=%2F${notFoundPath}`);
    expect(((await notFoundResolveResponse.json()) as { kind: string }).kind).toBe('NotFound');

    await page.getByRole('tab', { name: '404 yolları' }).click();
    await scanScreen(page, '404 yolları sekmesi');
    const notFoundRow = page.getByRole('row', { name: new RegExp(notFoundPath) });
    await expect(notFoundRow).toBeVisible();
    await notFoundRow.getByRole('button', { name: 'Yönlendirmeye çevir' }).click();
    const convertDialog = page.getByRole('dialog');
    await convertDialog.getByLabel('Hedef yol').fill(`e2e-notfound-target-${RUN_ID}`);
    await convertDialog.getByRole('button', { name: 'Yönlendirmeye çevir' }).click();
    await expect(page.getByText('Yönlendirmeye çevrildi.').last()).toBeVisible();
    await expect(notFoundRow).toHaveCount(0);

    await page.getByRole('tab', { name: 'Yönlendirmeler' }).click();
    const convertedRow = page.getByRole('row', { name: new RegExp(notFoundPath) });
    await expect(convertedRow).toBeVisible();
    const convertedListResponse = await request.get(`${BACKEND_URL}/api/v1/admin/website/redirects?search=${notFoundPath}`, { headers });
    const convertedList = (await convertedListResponse.json()) as { items: Array<{ id: string; fromPath: string }> };
    createdNotFoundRedirectId = convertedList.items.find((r) => r.fromPath === notFoundPath)?.id;
    notFoundLogId = undefined; // dönüştürme 404 kaydını zaten sildi.

    // Temizlik: yönlendirmeyi UI'dan sil (ikinci bir silme yolunu da kanıtlar).
    await redirectRow.getByRole('button', { name: 'Sil' }).click();
    await page.getByRole('button', { name: 'Onayla' }).click();
    await expect(page.getByText('Yönlendirme silindi.').last()).toBeVisible();
    createdRedirectId = undefined;
  });
});
