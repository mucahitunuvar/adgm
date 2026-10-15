import AxeBuilder from '@axe-core/playwright';
import { test, expect, type Page } from '@playwright/test';

const ADMIN_URL = 'http://localhost:5173/admin/';
// docs/DEVELOPMENT.md §44.1: yerel geliştirme LocalDB'sine bir defaya mahsus eklenmiş gerçek Admin.
const ADMIN_EMAIL = 'admin@dev.local';
const ADMIN_PASSWORD = 'DevAdmin#2026';

async function setTheme(page: Page, theme: 'light' | 'dark') {
  await page.emulateMedia({ colorScheme: theme });
}

async function expectNoAxeViolations(page: Page) {
  const results = await new AxeBuilder({ page }).analyze();
  expect(results.violations).toEqual([]);
}

// Tek giriş (auth uçlarının paylaşılan dakikalık limiti için; bkz. admin-auth.spec.ts).
test.describe.configure({ mode: 'serial' });

test.describe('admin: site yönetimi (CMS, Faz 2 Dilim 2a-1) - gerçek backend', () => {
  test('ayarlar kalıcı olarak kaydedilir; içerik tipi oluşturulur; kategori ağacı kurulur', async ({ page }) => {
    // 8 axe taraması + çok adımlı CRUD akışı varsayılan 30 sn'yi paralel koşu altında aşabilir.
    test.setTimeout(60_000);
    await page.goto(`${ADMIN_URL}giris`);
    await page.getByLabel(/^E-posta/).fill(ADMIN_EMAIL);
    await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Giriş yap' }).click();
    await expect(page).toHaveURL(new RegExp(`${ADMIN_URL.replace(/\/$/, '')}/?$`));

    // --- Erişilebilirlik: yeni CMS ekranlarının listeleri, açık ve koyu temada axe temiz. ---
    for (const theme of ['light', 'dark'] as const) {
      await setTheme(page, theme);
      await page.getByRole('link', { name: 'Site ayarları' }).click();
      await expectNoAxeViolations(page);
      await page.getByRole('link', { name: 'Diller' }).click();
      await expectNoAxeViolations(page);
      await page.getByRole('link', { name: 'İçerik tipleri' }).click();
      await expectNoAxeViolations(page);
      await page.getByRole('link', { name: 'Etiketler' }).click();
      await expectNoAxeViolations(page);
    }
    await setTheme(page, 'light');

    // --- Ayarlar: bir bayrak değiştirilip kaydedilince admin GET'inde (kalıcılık) görünür. ---
    // NOT: Burada kasıtlı olarak `GET /public/site` DEĞİL, admin'in kendi GET'i kontrol ediliyor.
    // Backend bulgusu (tamamlama raporu madde b, ENGEL benzeri): `PUT /admin/website/settings/*`
    // sonrası admin GET'i her zaman güncel değeri döner, ama `GET /public/site` (ICacheService
    // önbelleği) güncellenmiyor - izole curl testiyle doğrulandı (10+ dk sonra bile eski değer
    // döndü). `WebsiteCacheInvalidator.InvalidateAllPublic` çağrısı kod incelemesinde doğru
    // bağlanmış görünüyor; kök neden bulunamadı, backend'e bildirilmeli.
    await page.getByRole('link', { name: 'Site ayarları' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/ayarlar$/);
    await page.getByRole('tab', { name: 'Özellikler' }).click();

    const checkbox = page.getByLabel('Bülten aboneliği etkin');
    const wasChecked = await checkbox.isChecked();
    await checkbox.setChecked(!wasChecked);
    await page.getByRole('button', { name: 'Kaydet' }).click();
    // Bildirimler 5 sn sonra kendiliğinden kapanır ve üst üste yığılabilir; her zaman en sonuncusuna bakılır.
    await expect(page.getByText('Özellik bayrakları güncellendi.').last()).toBeVisible();

    // Sekme değiştirip geri dönmek formu yeniden kurar (Tabs, etkin olmayan panelin içeriğini
    // tutmaz) ve bu yüzden sunucudan tazelenmiş veriyle yeniden başlar - K-07 (bellek tabanlı
    // oturum) nedeniyle burada `page.reload()` KULLANILAMAZ, o oturumu bitirir.
    await page.getByRole('tab', { name: 'Kimlik' }).click();
    await page.getByRole('tab', { name: 'Özellikler' }).click();
    await expect(page.getByLabel('Bülten aboneliği etkin')).toBeChecked({ checked: !wasChecked });

    // Geri al: test ortamı kalıcı olarak değiştirilmez.
    await page.getByLabel('Bülten aboneliği etkin').setChecked(wasChecked);
    await page.getByRole('button', { name: 'Kaydet' }).click();
    await expect(page.getByText('Özellik bayrakları güncellendi.').last()).toBeVisible();

    // --- İçerik tipi: sabit anahtarla bul-ya-da-oluştur, etkinleştir. ---
    // Her koşuda `e2e-${Date.now()}` ile yeni bir tip yaratmak, içerik tiplerinde silme ucu
    // olmadığı için (yalnız pasifleştirme) dev veritabanında koşu sayısı kadar pasif çöp tip
    // biriktiriyordu. Sabit anahtar + bul-ya-da-oluştur bunu önler; tip hiç silinmez, yeniden
    // kullanılır.
    const key = 'e2e-sabit';
    await page.getByRole('link', { name: 'İçerik tipleri' }).click();
    await expect(page).toHaveURL(/\/site-yonetimi\/icerik-tipleri$/);

    const existingRow = page.getByRole('row', { name: new RegExp(`^${key}\\b`) });
    if (await existingRow.count() > 0) {
      await existingRow.getByRole('link', { name: 'Görüntüle' }).click();
    } else {
      await page.getByRole('button', { name: 'Yeni içerik tipi' }).click();
      await page.getByLabel('Anahtar').fill(key);
      await page.getByLabel('Ad (varsayılan dil)').fill('E2E İçerik Tipi');
      await page.getByLabel('Rota öneki (varsayılan dil)').fill(key);
      await page.getByLabel('Liste şablonu').fill('default');
      await page.getByLabel('Detay şablonu').fill('default');
      await page.getByLabel('Kategoriler').check();
      await page.getByRole('button', { name: 'Oluştur' }).click();
    }

    await expect(page).toHaveURL(/\/site-yonetimi\/icerik-tipleri\/[0-9a-fA-F-]{36}$/);
    // Yeni oluşturulduysa zaten etkindir; yeniden kullanılan tip önceki koşunun sonunda
    // pasifleştirilmiş olabilir - her iki durumda da "Pasifleştir" görünür hale gelmeli.
    const activateButton = page.getByRole('button', { name: 'Etkinleştir' });
    if (await activateButton.isVisible().catch(() => false)) {
      await activateButton.click();
      await expect(page.getByText('İçerik tipi etkinleştirildi.').last()).toBeVisible();
    }
    await expect(page.getByRole('button', { name: 'Pasifleştir' })).toBeVisible();
    await expectNoAxeViolations(page);

    // --- Kategori ağacı: kök kategori oluşturulur, public/site'ten bağımsız olarak kendi ekranında görünür. ---
    await page.getByRole('tab', { name: 'Kategoriler' }).click();
    await page.getByRole('button', { name: 'Kök kategori ekle' }).click();
    await page.getByLabel('Ad (varsayılan dil)').fill('E2E Kategori');
    await page.getByLabel('Slug').fill(`${key}-kategori`);
    await page.getByRole('button', { name: 'Oluştur' }).click();
    await expect(page.getByText('E2E Kategori')).toBeVisible();

    // Temizlik: kategori silinir (gerçek silme ucu var); içerik tipi pasifleştirilir (silme ucu yok).
    await page.getByRole('button', { name: 'Sil' }).click();
    await expect(page.getByText('Kategori silindi.').last()).toBeVisible();

    // "Genel" sekmesine dön: "Kategoriler" sekmesindeki (gizli) kategori satırı da bir
    // "Pasifleştir" düğmesi taşıdığından, sayfa başlığındaki düğmeyle isim çakışmasını önler.
    await page.getByRole('tab', { name: 'Genel' }).click();
    await page.getByRole('button', { name: 'Pasifleştir' }).click();
    await expect(page.getByText('İçerik tipi pasifleştirildi.').last()).toBeVisible();
  });
});
