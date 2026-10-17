import { snapshotSiteSettings } from './siteSettingsSnapshot';

// Bkz. siteSettingsSnapshot.ts'in başlık yorumu: paketteki herhangi bir spec'in kendi SiteSettings
// mutasyonundan tamamen bağımsız bir güvenlik ağı. Playwright bunu TÜM worker'lar/test dosyaları
// başlamadan ÖNCE, tek bir Node sürecinde bir kez çalıştırır.
export default async function globalSetup(): Promise<void> {
  await snapshotSiteSettings();
}
