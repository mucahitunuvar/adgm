/**
 * Yapıştırılan HTML, ProseMirror'ın kendi şema ayrıştırıcısına girmeden önce burada süzülür.
 * `image`/`figure`/`figcaption` şemada TANIMLI oldukları için (backend whitelist'inde var),
 * ProseMirror'ın varsayılan ayrıştırması bunları normal şekilde kabul ederdi - ama görsel eklemenin
 * TEK yolu MediaPicker'dan geçen `insertFigureImage` komutu olmalı (gövdede serbest harici görsel
 * URL'si yok). Bu yüzden yapıştırılan HTML'den her ikisi de tamamen çıkarılır; şemadaki diğer her
 * şey (p/strong/em/başlık/liste/tablo/bağlantı vb.) ProseMirror'ın kendi ayrıştırıcısına bırakılır -
 * o da zaten şemada karşılığı olmayan etiketleri (div/span/style/script vb.) kendiliğinden düşürür.
 */
export function transformPastedHtml(html: string): string {
  const doc = new DOMParser().parseFromString(html, 'text/html');
  doc.querySelectorAll('img, figure, figcaption').forEach((node) => node.remove());
  return Array.from(doc.body.childNodes)
    .map((node) => new XMLSerializer().serializeToString(node))
    .join('');
}
