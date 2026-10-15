import { useQuery } from '@tanstack/react-query';
import { ApiError, getPublicSite } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { ErrorState, LoadingState, PageHeader } from '@gm/ui';

export function ConnectionTestPage() {
  const { client } = useAuth();
  const { data, error, isLoading } = useQuery({
    queryKey: ['public-site'],
    queryFn: () => getPublicSite(client),
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Faz 0"
        title="Bağlantı testi"
        description="GET /api/v1/public/site yanıtı backend'den canlı olarak çekilir. Gerçek giriş ve profil akışı Faz 1'de eklenecek."
      />
      {isLoading && <LoadingState />}
      {error && <ErrorState description={error instanceof ApiError ? error.title : 'Beklenmeyen bir hata oluştu.'} />}
      {!isLoading && !error && <pre tabIndex={0}>{JSON.stringify(data, null, 2)}</pre>}
    </div>
  );
}
