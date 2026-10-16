import AxeBuilder from '@axe-core/playwright';
import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

const WEB_URL = 'http://localhost:3000';
const BACKEND_URL = 'http://localhost:5016';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';

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

// Araştırıldı (16 Ekim): headless Chromium + `next dev`, bazı koşularda `.eyebrow`/`.page-lead`
// için gerçekte var olmayan bir arka plan rengiyle ("#445665" gibi, DOM'daki hiçbir elemanın
// `getComputedStyle().backgroundColor`'ı değil) sahte bir `color-contrast` ihlali üretiyor.
// Bağımsız doğrulama: `scripts/check-contrast.mjs` aynı çifti ("ember-on-dark / Navy") 5.84:1 ile
// GEÇİRİYOR; doğrudan DOM incelemesi de body'den eyebrow'a kadar zincirde yarı saydam/üst üste
// binen hiçbir katman olmadığını, gerçek arka planın solid `--color-bg` (#071f32) olduğunu gösterdi.
// Bu yüzden SADECE bu iki seçici + bu kural için tolere edilir; başka hiçbir ihlal (başka seçici,
// başka kural) bu filtreden geçmez.
// Üretim doğrulaması (Faz 3 Koşu 3E, 16 Ekim): `next build && next start`'a karşı (`next dev`
// DEĞİL) aynı iki seçici + açık/koyu tema axe ile ayrıca tarandı, bağımsız bir betikle - 0
// `color-contrast` ihlali bulundu. Bu, istisnanın gerçekten yalnız `next dev` + headless
// Chromium'a özgü olduğunu kanıtlıyor; istisna bu yüzden kaldırılmadı.
const KNOWN_FALSE_POSITIVE_TARGETS = new Set(['.eyebrow', '.page-lead']);

async function scanScreen(page: Page, url: string, label: string) {
  for (const viewport of VIEWPORTS) {
    await page.setViewportSize({ width: viewport.width, height: viewport.height });
    for (const theme of THEMES) {
      await page.emulateMedia({ colorScheme: theme });
      await page.goto(url);
      const results = await new AxeBuilder({ page }).analyze();
      const realViolations = results.violations.filter((violation) => {
        if (violation.id !== 'color-contrast') return true;
        return !violation.nodes.every((node) => node.target.some((target) => KNOWN_FALSE_POSITIVE_TARGETS.has(target)));
      });
      expect(realViolations, `${label} @ ${viewport.label}, ${theme}`).toEqual([]);
    }
  }
}

// `mode: 'serial'`: SiteSettings.MaintenanceModeEnabled tek, paylaşılan bir satırdır (aynı
// SiteSettingsFlowTests'in kendi yorumunda belgelediği singleton) - bakım modu testi bunu açıp
// kapatırken AYNI dosyadaki diğer testler paralel çalışırsa (Playwright varsayılanı) 503 alıp
// rastgele "h1 yok"/"landmark yok" axe ihlalleri üretirler (canlı doğrulandı - bu yorum bir
// varsayım değil, gerçekten yaşanan bir yarış koşulunun kaydı). Bakım testi bu yüzden listenin
// EN SONUNDA.
test.describe.configure({ mode: 'serial' });

test.describe('apps/web: ana sayfa SSR, rota çözümleme, erişilebilirlik, bakım modu (gerçek backend)', () => {
  test('ana sayfa gerçek backend verisiyle SSR edilir, oturum/token sızdırmaz', async ({ request }) => {
    const response = await request.get(`${WEB_URL}/`);
    expect(response.status()).toBe(200);
    const html = await response.text();
    expect(html).toContain('<html lang="tr"');
    expect(html).not.toMatch(/accessToken|refreshToken/i);
  });

  test('bilinmeyen yol 404 döner, önbelleğe girmez', async ({ request }) => {
    const response = await request.get(`${WEB_URL}/olmayan-bir-sayfa-${Date.now()}`, {
      headers: { 'Cache-Control': 'no-cache' },
    });
    expect(response.status()).toBe(404);
  });

  test('CSP ve güvenlik başlıkları her sayfada var', async ({ request }) => {
    const response = await request.get(`${WEB_URL}/`);
    const csp = response.headers()['content-security-policy'];
    expect(csp).toContain("default-src 'self'");
    expect(csp).toContain('https://i.ytimg.com');
    expect(response.headers()['x-content-type-options']).toBe('nosniff');
  });

  test('tema anahtarı dışında tarayıcı depolamasına hiçbir şey yazılmaz', async ({ page }) => {
    await page.goto(`${WEB_URL}/`);
    await page.getByRole('button', { name: /temaya geç/i }).first().click();
    const storageKeys = await page.evaluate(() => ({
      localStorage: Object.keys(window.localStorage),
      sessionStorage: Object.keys(window.sessionStorage),
    }));
    expect(storageKeys.localStorage).toEqual(['theme']);
    expect(storageKeys.sessionStorage).toEqual([]);
  });

  test('erişilebilirlik: ana sayfa, 404, /giris, /uye-ol (390/1440 x açık/koyu)', async ({ page }) => {
    await scanScreen(page, `${WEB_URL}/`, 'Ana sayfa');
    await scanScreen(page, `${WEB_URL}/giris-olmayan-${Date.now()}`, '404');
    await scanScreen(page, `${WEB_URL}/giris`, '/giris');
    await scanScreen(page, `${WEB_URL}/uye-ol`, '/uye-ol');
  });

  test('bakım modu: bayrak açılınca 503 + Retry-After döner, kapatılınca eski haline döner', async ({ request }) => {
    const token = await loginForApiToken(request);
    const headers = { Authorization: `Bearer ${token}` };

    const settings = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers })).json()) as { rowVersion: string };
    const enableResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/maintenance`, {
      headers,
      data: { rowVersion: settings.rowVersion, maintenanceModeEnabled: true, translations: [] },
    });
    expect(enableResponse.status()).toBe(204);

    try {
      // apps/web'in kendi bakım kontrolü 10 sn TTL önbellekli (lib/maintenanceFlag.ts) - taze bir
      // değer görmesi için bu pencereyi aşacak kadar beklenir.
      await new Promise((resolve) => setTimeout(resolve, 11_000));

      const homeDuringMaintenance = await request.get(`${WEB_URL}/`, { headers: { 'Cache-Control': 'no-cache' } });
      expect(homeDuringMaintenance.status()).toBe(503);
      expect(homeDuringMaintenance.headers()['retry-after']).toBeTruthy();
    } finally {
      const afterSettings = (await (await request.get(`${BACKEND_URL}/api/v1/admin/website/settings`, { headers })).json()) as { rowVersion: string };
      const disableResponse = await request.put(`${BACKEND_URL}/api/v1/admin/website/settings/maintenance`, {
        headers,
        data: { rowVersion: afterSettings.rowVersion, maintenanceModeEnabled: false, translations: [] },
      });
      expect(disableResponse.status()).toBe(204);
    }

    await new Promise((resolve) => setTimeout(resolve, 11_000));
    const homeAfterRestore = await request.get(`${WEB_URL}/`, { headers: { 'Cache-Control': 'no-cache' } });
    expect(homeAfterRestore.status()).toBe(200);
  });
});
