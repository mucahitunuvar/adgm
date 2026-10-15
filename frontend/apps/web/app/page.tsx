import { ApiError, createApiClient, getPublicHome, getPublicSite } from '@gm/api-client';

const backendOrigin = process.env.BACKEND_ORIGIN ?? 'http://localhost:5016';

export default async function HomePage() {
  const client = createApiClient({ baseUrl: backendOrigin });

  let site: unknown = null;
  let home: unknown = null;
  let errorMessage: string | null = null;

  try {
    [site, home] = await Promise.all([getPublicSite(client), getPublicHome(client)]);
  } catch (error) {
    errorMessage =
      error instanceof ApiError
        ? `${error.title}${error.correlationId ? ` (destek kodu: ${error.correlationId})` : ''}`
        : 'Beklenmeyen bir hata oluştu.';
  }

  const isEmpty = !errorMessage && !site && !home;

  return (
    <main>
      <div className="container section">
        <p className="eyebrow">Faz 0 · Bağlantı testi</p>
        <h1>Ahmet Demir Gençlik Merkezi</h1>
        <p className="page-lead">
          Bu sayfa gerçek backend'den dönen <code>GET /api/v1/public/site</code> ve{' '}
          <code>GET /api/v1/public/home</code> yanıtlarını olduğu gibi gösterir. Public site henüz CMS
          verisiyle çizilmiyor (bu Faz 3 işidir); oturum okumaz.
        </p>

        {errorMessage && (
          <div className="page-state page-state--error" role="alert">
            <h2>Backend'e bağlanılamadı</h2>
            <p>{errorMessage}</p>
          </div>
        )}

        {!errorMessage && isEmpty && (
          <div className="page-state">
            <h2>Veri yok</h2>
            <p>Backend boş bir yanıt döndürdü.</p>
          </div>
        )}

        {!errorMessage && !isEmpty && (
          <>
            <section>
              <h2>GET /api/v1/public/site</h2>
              <pre tabIndex={0}>{JSON.stringify(site, null, 2)}</pre>
            </section>
            <section>
              <h2>GET /api/v1/public/home</h2>
              <pre tabIndex={0}>{JSON.stringify(home, null, 2)}</pre>
            </section>
          </>
        )}
      </div>
    </main>
  );
}
