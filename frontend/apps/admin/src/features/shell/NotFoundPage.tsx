import { PageHeader, buttonClassName } from '@gm/ui';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../routes';

export function NotFoundPage() {
  return (
    <main className="container section auth-page">
      <PageHeader title="Sayfa bulunamadı" description="Aradığınız sayfa taşınmış veya hiç var olmamış olabilir." />
      <Link to={ROUTES.dashboard} className={buttonClassName('primary')}>
        Panele dön
      </Link>
    </main>
  );
}
