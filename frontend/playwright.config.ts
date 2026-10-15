import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
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
