// Next.js middleware statüs kodunu kendi başına değiştiremeyen bir App Router sayfası render
// edemez (notFound()/redirect() dışında özel bir durum fonksiyonu yok); bu yüzden gerçek 503 +
// Retry-After header'ı burada, middleware'den dönen düz HTML ile sağlanıyor - React/tokens.css
// bilerek kullanılmıyor (middleware Edge ortamında çalışır, bu paketleri import edemez).
// Palet FRONTEND-TASARIM-SISTEMI.md / K-04 ile birebir, yalnız bu tek sayfa için sabitlendi.
export function renderMaintenancePageHtml(message: string): string {
  const safeMessage = message
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');

  return `<!doctype html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<meta name="robots" content="noindex" />
<title>Bakımdayız — Ahmet Demir Gençlik Merkezi</title>
<style>
  :root { --bg: #e6efed; --fg: #071f32; --accent: #c44325; --card-bg: #ffffff; }
  @media (prefers-color-scheme: dark) {
    :root { --bg: #041725; --fg: #e6efed; --accent: #d94a2b; --card-bg: #102a2e; }
  }
  * { box-sizing: border-box; }
  body {
    margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center;
    background: var(--bg); color: var(--fg); padding: 16px;
    font-family: 'Manrope', system-ui, -apple-system, Segoe UI, sans-serif;
  }
  main {
    max-width: 480px; width: 100%; background: var(--card-bg); border-radius: 24px;
    padding: 32px; text-align: center; box-shadow: 0 1px 3px rgba(0,0,0,0.12);
  }
  h1 { font-size: 1.5rem; margin: 0 0 12px; }
  p { line-height: 1.6; margin: 0; }
  .accent { color: var(--accent); font-weight: 700; }
</style>
</head>
<body>
<main role="alert">
  <h1>Bakım çalışması <span class="accent">sürüyor</span></h1>
  <p>${safeMessage}</p>
</main>
</body>
</html>`;
}
