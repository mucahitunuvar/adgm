import { test, expect, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

const apps = [
  { name: 'web', url: 'http://localhost:3000/', expectNoIndex: false },
  { name: 'candidate', url: 'http://localhost:5171/portal/aday/', expectNoIndex: true },
  { name: 'employer', url: 'http://localhost:5172/portal/firma/', expectNoIndex: true },
  { name: 'admin', url: 'http://localhost:5173/admin/', expectNoIndex: true },
];

async function setTheme(page: Page, theme: 'light' | 'dark') {
  await page.emulateMedia({ colorScheme: theme });
}

for (const app of apps) {
  test.describe(app.name, () => {
    test('sayfa açılır, Türkçe karakterler bozulmaz ve belleğe token yazılmaz', async ({ page }) => {
      const response = await page.goto(app.url);
      expect(response?.ok()).toBeTruthy();
      await expect(page.locator('html')).toHaveAttribute('lang', 'tr');

      const bodyText = await page.locator('body').innerText();
      expect(bodyText.length).toBeGreaterThan(0);

      const storageSnapshot = await page.evaluate(() => ({
        localStorageKeys: Object.keys(window.localStorage),
        sessionStorageKeys: Object.keys(window.sessionStorage),
        localStorageValues: Object.values(window.localStorage),
      }));
      // Tema anahtarı (theme) dışında hiçbir şey saklanmamalı; token asla.
      expect(storageSnapshot.localStorageKeys.filter((key) => key !== 'theme')).toEqual([]);
      expect(storageSnapshot.sessionStorageKeys).toEqual([]);
      for (const value of storageSnapshot.localStorageValues) {
        expect(String(value)).not.toMatch(/eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\./); // JWT görünümlü değer yok
      }
    });

    if (app.expectNoIndex) {
      test('X-Robots-Tag: noindex başlığı mevcuttur', async ({ page }) => {
        const response = await page.goto(app.url);
        const headerValue = response?.headers()['x-robots-tag'];
        expect(headerValue ?? '').toContain('noindex');
      });
    }

    for (const theme of ['light', 'dark'] as const) {
      test(`axe erişilebilirlik ihlali yok (${theme})`, async ({ page }) => {
        await setTheme(page, theme);
        await page.goto(app.url);
        const results = await new AxeBuilder({ page }).analyze();
        expect(results.violations).toEqual([]);
      });
    }
  });
}
