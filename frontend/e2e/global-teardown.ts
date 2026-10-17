import { restoreSiteSettings } from './siteSettingsSnapshot';

// Playwright, `globalSetup` başarıyla BİTTİYSE bunu HER ZAMAN çalıştırır - paket içindeki herhangi
// bir test/hook çökse, hatta `globalTimeout` paketin tamamını erken kesse bile (bkz.
// siteSettingsSnapshot.ts'in başlık yorumu).
export default async function globalTeardown(): Promise<void> {
  await restoreSiteSettings();
}
