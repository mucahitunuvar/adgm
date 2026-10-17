import { defineConfig, devices } from '@playwright/test';

// KOŞU 3G Görev 4 (e2e kararlılığı): her spec dosyası ZATEN kendi içinde `test.describe.configure({
// mode: 'serial' })` kullanıyor (paylaşılan kurulum/temizlik için), ama bu yalnız BİR dosya İÇİNDEKİ
// testleri sıralar - `fullyParallel: true` + Playwright'ın varsayılan (CPU sayısına bağlı) worker
// sayısıyla FARKLI dosyalar hâlâ aynı anda farklı worker'larda çalışabilir. Bu proje bunun gerçek
// sonuçlarını zaten yaşadı (bkz. FRONTEND-ACIK-MADDELER.md madde 30): paralel koşulduğunda backend'in
// paylaşılan dakikalık `authenticated` rate limit bütçesi GERÇEKTEN tükeniyor (gözlemlenmiş 429'lar)
// ve `web-home.spec.ts`/`web-content.spec.ts` gibi dosyalar AYNI tekil SiteSettings satırını (bakım
// modu, bot koruması, çerez politikası) paralel açıp kapatınca yarış koşulları üretiyor. `workers: 1`
// + `fullyParallel: false` TÜM paketi (dosyalar arası dahil) sıraya alır - tek doğru düzeltme bu,
// çünkü backend'in paylaşılan durumu paketin HİÇBİR şekilde güvenle paralelleştirilemeyeceği anlamına
// gelir (yalnız `smoke.spec.ts` durum değiştirmez, ama ayrı bir proje/worker payı tanımlamanın karmaşıklığı
// kazandırdığından daha fazlasına mal olur - bu küçük dosya için konfigürasyon karmaşıklığı yerine
// doğruluk tercih edildi).
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  // Varsayılan (30 sn) gerçek backend'e karşı birden çok round-trip yapan testler için dar; büyük
  // senaryolar (örn. `web-content.spec.ts`'in kurulumu) kendi `test.setTimeout(...)`'unu zaten
  // ayarlıyor - bu yalnız genel varsayılanı yükseltiyor, en uzun özel aşımların (300 sn) ÜSTÜNE
  // çıkmıyor.
  timeout: 60_000,
  // Paketin TAMAMI için sert bir üst sınır: madde 30'daki gerçek bir koşuda paket 11.4 SAAT sürüp 7
  // test düşmüştü (muhtemelen tam paralel + paylaşılan rate limit/SiteSettings yarışlarının bir
  // çöküş-geri çekilme sarmalı - bkz. madde 30'un kendi notları). `workers: 1` bu kök nedeni ortadan
  // kaldırıyor, ama bu sınır bir REGRESYON KORUMASI: gelecekte benzer bir şey tekrar olursa paket
  // saatlerce değil, en fazla burada verilir süre kadar çalışıp AÇIKÇA başarısız olur.
  globalTimeout: 30 * 60_000,
  globalSetup: './e2e/global-setup.ts',
  globalTeardown: './e2e/global-teardown.ts',
  reporter: 'list',
  use: {
    trace: 'retain-on-failure',
  },
  webServer: [
    { command: 'pnpm --filter @gm/web dev', url: 'http://localhost:3000', reuseExistingServer: true, timeout: 60_000 },
    { command: 'pnpm --filter @gm/candidate dev', url: 'http://localhost:5171/portal/aday/', reuseExistingServer: true, timeout: 60_000 },
    { command: 'pnpm --filter @gm/employer dev', url: 'http://localhost:5172/portal/firma/', reuseExistingServer: true, timeout: 60_000 },
    { command: 'pnpm --filter @gm/admin dev', url: 'http://localhost:5173/admin/', reuseExistingServer: true, timeout: 60_000 },
  ],
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
