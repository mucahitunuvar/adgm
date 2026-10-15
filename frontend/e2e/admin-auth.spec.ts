import { test, expect, type APIRequestContext } from '@playwright/test';

const BACKEND_ORIGIN = 'http://localhost:5016';
const ADMIN_URL = 'http://localhost:5173/admin/';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';

// Backend "auth" rate-limit politikası (Program.cs) geliştirmede IP başına 5 istek/dakika ile
// login/register/forgot-password/... uçlarının TAMAMINI paylaşır. Bu dosyadaki testler bu yüzden
// seri çalışır ve bu bütçeyi tüketen çağrı sayısını kasıtlı olarak düşük tutar (en fazla 1 kayıt +
// 1 admin girişi + 1 aday girişi = 3 çağrı). Yanlış parola senaryosu burada tekrar denenmez;
// LoginPage.test.tsx zaten sahte (mock) bir backend'le kapsıyor.
test.describe.configure({ mode: 'serial' });

let candidate: { email: string; password: string };

async function registerDisposableCandidate(request: APIRequestContext) {
  const email = `e2e-admin-faz1-${Date.now()}@example.com`;
  const response = await request.post(`${BACKEND_ORIGIN}/api/v1/candidates/register`, {
    data: { email, password: 'TestCandidate1', firstName: 'E2E', lastName: 'Test', phoneNumber: null },
  });
  expect(response.ok(), 'disposable candidate registration should succeed').toBeTruthy();
  return { email, password: 'TestCandidate1' };
}

test.describe('admin: giriş akışı ve yönetim (gerçek backend)', () => {
  test.beforeAll(async ({ request }) => {
    candidate = await registerDisposableCandidate(request);
  });

  test('Admin girişi, kullanıcı listesi, rol değiştirme (geri alınır), çıkış ve oturumun yenilemede bitmesi', async ({ page }) => {
    // Oturum açılmadan korumalı rotaya gidilirse girişe yönlendirilir.
    await page.goto(`${ADMIN_URL}hesabim`);
    await expect(page).toHaveURL(/\/admin\/giris$/);

    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();

    // Girişten önce istenen sayfa /hesabim'di; başarılı girişte oraya (panele değil) döner.
    await expect(page).toHaveURL(/\/admin\/hesabim$/);
    await expect(page.getByRole('heading', { name: 'Hesap bilgileri' })).toBeVisible();

    await page.getByRole('link', { name: 'Panel' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`));
    await expect(page.getByText(`Hoş geldiniz, ${ADMIN_EMAIL}`)).toBeVisible();

    // Kullanıcılar: disposable aday filtrelenip bulunabiliyor.
    await page.getByRole('link', { name: 'Kullanıcılar', exact: true }).click();
    await expect(page).toHaveURL(/\/admin\/kullanicilar$/);
    await page.getByLabel('E-posta').fill(candidate.email);
    await page.getByRole('button', { name: 'Filtrele' }).click();
    await expect(page.getByRole('cell', { name: candidate.email })).toBeVisible();

    // Detay: rolü CareerAdvisor'a değiştir, sonra Candidate'a geri al.
    await page.getByRole('link', { name: 'Görüntüle' }).click();
    await expect(page.getByRole('heading', { name: candidate.email })).toBeVisible();

    // Seçenek `value`'ları backend üye adıdır (ör. "CareerAdvisor"); arayüzde gösterilen
    // metin ise çeviridir (enum-labels.tr.ts, Görev 0b) - ham enum adı asla gösterilmez.
    await page.getByRole('button', { name: 'Rolü değiştir' }).click();
    await page.getByLabel('Yeni rol').selectOption('CareerAdvisor');
    await page.getByRole('button', { name: 'Rolü güncelle' }).click();
    // Bildirimler 5 sn sonra kendiliğinden kapanır ve üst üste yığılabilir (bkz. diğer spec'lerdeki
    // aynı not) - her zaman en sonuncusuna bakılır, aksi halde ikinci rol değişikliğinde iki bildirim
    // aynı anda DOM'da olup "strict mode violation" ile düşer (madde 30'da kayıtlı, 15 Ekim'de kapandı).
    await expect(page.getByText('Kullanıcının rolü güncellendi.').last()).toBeVisible();
    await expect(page.getByText('Kariyer danışmanı', { exact: true }).first()).toBeVisible();

    await page.getByRole('button', { name: 'Rolü değiştir' }).click();
    await page.getByLabel('Yeni rol').selectOption('Candidate');
    await page.getByRole('button', { name: 'Rolü güncelle' }).click();
    await expect(page.getByText('Kullanıcının rolü güncellendi.').last()).toBeVisible();
    await expect(page.getByText('Aday', { exact: true }).first()).toBeVisible();

    // Denetim kaydı: bu kullanıcı için en az bir RoleChanged (üye adı "RoleChanged",
    // etiketi "Rol değiştirildi") kaydı görünür.
    await page.getByRole('link', { name: 'Denetim kaydını görüntüle' }).click();
    await expect(page).toHaveURL(/\/admin\/denetim-kaydi\?userId=/);
    await expect(page.getByRole('cell', { name: 'Rol değiştirildi' }).first()).toBeVisible();

    // Çıkış: bellek temizlenir, girişe döner, hiçbir depoya token yazılmamıştır.
    await page.getByRole('button', { name: 'Çıkış yap' }).click();
    await expect(page).toHaveURL(/\/admin\/giris$/);

    const storageSnapshot = await page.evaluate(() => ({
      localStorageKeys: Object.keys(window.localStorage),
      sessionStorageKeys: Object.keys(window.sessionStorage),
    }));
    expect(storageSnapshot.localStorageKeys.filter((key) => key !== 'theme')).toEqual([]);
    expect(storageSnapshot.sessionStorageKeys).toEqual([]);

    // K-07: sayfa yenilenince oturum biter (ikinci bir giriş isteği olmadan, zaten girişteyiz).
    await page.goto(`${ADMIN_URL}`);
    await expect(page).toHaveURL(/\/admin\/giris$/);
  });

  test('Candidate rolüyle giriş admin panelinde "yetkiniz yok" gösterir, çıkış yapılabilir', async ({ page }) => {
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(candidate.email);
    await page.locator('input[type="password"]').fill(candidate.password);
    await page.getByRole('button', { name: 'Giriş yap' }).click();

    await expect(page).toHaveURL(/\/admin\/yetkisiz$/);
    await expect(page.getByText('Bu panele yetkiniz yok')).toBeVisible();

    await page.getByRole('button', { name: 'Çıkış yap' }).click();
    await expect(page).toHaveURL(/\/admin\/giris$/);
  });
});
