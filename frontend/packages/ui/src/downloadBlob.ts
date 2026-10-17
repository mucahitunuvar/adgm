/**
 * Yetkili bir `Blob`'u tarayıcıya indirir (FRONTEND-GUVENLIK.md §5: `<a href>` ile doğrudan indirme
 * yok, token yalnız bellekte). Geçici `objectURL` hemen sonra serbest bırakılır; dosya satır içinde
 * açılmaz (`<a>` öğesi DOM'a hiç eklenmez, yalnız programatik `click()` tetiklenir).
 */
export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  // İndirmeyi hemen ardından `revokeObjectURL` çağırmak bazı tarayıcılarda (indirme kendi
  // navigasyonunu asenkron başlattığı için) indirmeyi iptal edebilir; kısa bir gecikme güvenli.
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
