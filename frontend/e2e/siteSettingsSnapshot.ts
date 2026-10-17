import { tmpdir } from 'os';
import { join } from 'path';
import { readFileSync, writeFileSync, existsSync, unlinkSync } from 'fs';
import { request as playwrightRequest } from '@playwright/test';

// KOŞU 3G Görev 4 (e2e kararlılığı): her spec dosyasının kendi `beforeAll`/`afterAll` çifti SiteSettings'i
// değiştirip geri alıyor, ama bu ÇİFT kırılgan - `beforeAll` yarı yolda çökerse (ağ hatası, bir
// `expect` düşmesi) kendi `afterAll`'u hiç çalışmayabilir ya da yanlış/eksik bir durumdan geri almaya
// çalışabilir. Bu modül, TÜM paketten ÖNCE bir kez (`globalSetup`) SiteSettings'in mutasyona açık
// alanlarının anlık görüntüsünü alıp TÜM paketten SONRA bir kez (`globalTeardown`) geri yükleyen,
// her spec dosyasının kendi mekanizmasından TAMAMEN BAĞIMSIZ bir ikinci güvenlik ağı sağlar.
// Playwright, `globalSetup` başarıyla tamamlandıysa `globalTeardown`'ı HER ZAMAN çalıştırır - paket
// içindeki herhangi bir `beforeAll`/test çökse, hatta `globalTimeout` paketin tamamını kesip atsa bile.
const BACKEND_URL = 'http://localhost:5016';
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';
const SNAPSHOT_PATH = join(tmpdir(), 'gm-e2e-site-settings-snapshot.json');

interface TranslationFields {
  languageCode: string;
  maintenanceMessage: string;
  cookieBannerTitle: string;
  cookieBannerText: string;
  cookieCategoryNecessaryDescription: string;
  cookieCategoryAnalyticsDescription: string;
  cookieCategoryMarketingDescription: string;
}

interface SiteSettingsSnapshot {
  globalSearchEnabled: boolean;
  newsletterEnabled: boolean;
  publicJobListingsEnabled: boolean;
  donationPageEnabled: boolean;
  allowSearchEngineIndexing: boolean;
  botProtectionEnabled: boolean;
  turnstileSiteKey: string;
  cookiePolicyKey: string | null;
  maintenanceModeEnabled: boolean;
  translations: TranslationFields[];
}

interface SiteSettingsResponse {
  rowVersion: string;
  globalSearchEnabled: boolean;
  newsletterEnabled: boolean;
  publicJobListingsEnabled: boolean;
  donationPageEnabled: boolean;
  allowSearchEngineIndexing: boolean;
  botProtectionEnabled: boolean;
  turnstileSiteKey: string;
  cookiePolicyKey: string | null;
  maintenanceModeEnabled: boolean;
  translations: TranslationFields[];
}

async function loginAdmin(): Promise<{ token: string; dispose: () => Promise<void> }> {
  const context = await playwrightRequest.newContext();
  const response = await context.post(`${BACKEND_URL}/api/v1/auth/login`, {
    data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
  });
  const body = (await response.json()) as { accessToken: string };
  return { token: body.accessToken, dispose: () => context.dispose() };
}

export async function snapshotSiteSettings(): Promise<void> {
  const { token, dispose } = await loginAdmin();
  try {
    const context = await playwrightRequest.newContext({ baseURL: BACKEND_URL, extraHTTPHeaders: { Authorization: `Bearer ${token}` } });
    try {
      const settings = (await (await context.get('/api/v1/admin/website/settings')).json()) as SiteSettingsResponse;
      const snapshot: SiteSettingsSnapshot = {
        globalSearchEnabled: settings.globalSearchEnabled,
        newsletterEnabled: settings.newsletterEnabled,
        publicJobListingsEnabled: settings.publicJobListingsEnabled,
        donationPageEnabled: settings.donationPageEnabled,
        allowSearchEngineIndexing: settings.allowSearchEngineIndexing,
        botProtectionEnabled: settings.botProtectionEnabled,
        turnstileSiteKey: settings.turnstileSiteKey,
        cookiePolicyKey: settings.cookiePolicyKey,
        maintenanceModeEnabled: settings.maintenanceModeEnabled,
        translations: settings.translations,
      };
      writeFileSync(SNAPSHOT_PATH, JSON.stringify(snapshot), 'utf8');
    } finally {
      await context.dispose();
    }
  } finally {
    await dispose();
  }
}

export async function restoreSiteSettings(): Promise<void> {
  // Önceki bir koşudan kalan bir dosya yoksa (globalSetup hiç çalışmadıysa/henüz hiç koşulmadıysa)
  // geri yüklenecek bir şey yoktur - sessizce çıkılır, hataya düşürülmez.
  if (!existsSync(SNAPSHOT_PATH)) return;

  const snapshot = JSON.parse(readFileSync(SNAPSHOT_PATH, 'utf8')) as SiteSettingsSnapshot;
  const { token, dispose } = await loginAdmin();
  try {
    const context = await playwrightRequest.newContext({ baseURL: BACKEND_URL, extraHTTPHeaders: { Authorization: `Bearer ${token}` } });
    try {
      async function currentRowVersion(): Promise<string> {
        const settings = (await (await context.get('/api/v1/admin/website/settings')).json()) as SiteSettingsResponse;
        return settings.rowVersion;
      }

      await context.put('/api/v1/admin/website/settings/features', {
        data: {
          rowVersion: await currentRowVersion(),
          globalSearchEnabled: snapshot.globalSearchEnabled,
          newsletterEnabled: snapshot.newsletterEnabled,
          publicJobListingsEnabled: snapshot.publicJobListingsEnabled,
          donationPageEnabled: snapshot.donationPageEnabled,
          allowSearchEngineIndexing: snapshot.allowSearchEngineIndexing,
        },
      });

      await context.put('/api/v1/admin/website/settings/bot-protection', {
        data: {
          rowVersion: await currentRowVersion(),
          botProtectionEnabled: snapshot.botProtectionEnabled,
          turnstileSiteKey: snapshot.turnstileSiteKey,
        },
      });

      await context.put('/api/v1/admin/website/settings/cookie-consent', {
        data: {
          rowVersion: await currentRowVersion(),
          cookiePolicyKey: snapshot.cookiePolicyKey,
          translations: snapshot.translations.map((t) => ({
            languageCode: t.languageCode,
            cookieBannerTitle: t.cookieBannerTitle,
            cookieBannerText: t.cookieBannerText,
            cookieCategoryNecessaryDescription: t.cookieCategoryNecessaryDescription,
            cookieCategoryAnalyticsDescription: t.cookieCategoryAnalyticsDescription,
            cookieCategoryMarketingDescription: t.cookieCategoryMarketingDescription,
          })),
        },
      });

      await context.put('/api/v1/admin/website/settings/maintenance', {
        data: {
          rowVersion: await currentRowVersion(),
          maintenanceModeEnabled: snapshot.maintenanceModeEnabled,
          translations: snapshot.translations.map((t) => ({ languageCode: t.languageCode, maintenanceMessage: t.maintenanceMessage })),
        },
      });
    } finally {
      await context.dispose();
    }
  } finally {
    await dispose();
    // Bir sonraki koşunun eski bir anlık görüntüyü yanlışlıkla geri yüklemesini önler.
    if (existsSync(SNAPSHOT_PATH)) unlinkSync(SNAPSHOT_PATH);
  }
}
