import { ChevronLeft, ChevronRight } from 'lucide-react';
import { safeUrl } from '@gm/ui';

interface LinkPaginationProps {
  basePath: string;
  page: number;
  pageSize: number;
  totalCount: number;
  query?: Readonly<Record<string, string | undefined>>;
}

/**
 * `packages/ui` `Pagination`'ın `onPageChange` tabanlı sürümü istemci state'i gerektirir;
 * public sitede sayfalama taranabilir/JS'siz `<a href>` olmalıdır - bu yüzden ayrı bir bileşen.
 */
export function LinkPagination({ basePath, page, pageSize, totalCount, query = {} }: LinkPaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (totalPages <= 1) return null;

  function hrefFor(targetPage: number): string | undefined {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query)) {
      if (value) params.set(key, value);
    }
    if (targetPage > 1) params.set('page', String(targetPage));
    const search = params.toString();
    return safeUrl(search ? `${basePath}?${search}` : basePath);
  }

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);
  const previousHref = page > 1 ? hrefFor(page - 1) : undefined;
  const nextHref = page < totalPages ? hrefFor(page + 1) : undefined;

  return (
    <nav className="pagination" aria-label="Sayfalama">
      <p className="pagination__summary">
        {totalCount} kayıttan {from}–{to}
      </p>
      <div className="pagination__controls">
        {previousHref ? (
          <a href={previousHref} className="button button--quiet" aria-label="Önceki sayfa">
            <ChevronLeft aria-hidden="true" />
          </a>
        ) : (
          <span className="button button--quiet" aria-disabled="true">
            <ChevronLeft aria-hidden="true" />
          </span>
        )}
        <span className="pagination__page">
          {page} / {totalPages}
        </span>
        {nextHref ? (
          <a href={nextHref} className="button button--quiet" aria-label="Sonraki sayfa">
            <ChevronRight aria-hidden="true" />
          </a>
        ) : (
          <span className="button button--quiet" aria-disabled="true">
            <ChevronRight aria-hidden="true" />
          </span>
        )}
      </div>
    </nav>
  );
}
