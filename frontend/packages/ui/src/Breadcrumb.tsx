import { safeUrl } from './safeUrl';

export interface BreadcrumbItem {
  title: string;
  path: string;
}

/** Backend'in döndürdüğü hazır breadcrumb listesini basar (ADR-024 §15); frontend hiyerarşi hesaplamaz. */
export function Breadcrumb({ items, className = '' }: { items: readonly BreadcrumbItem[]; className?: string }) {
  if (items.length === 0) return null;
  return (
    <nav aria-label="Breadcrumb" className={`breadcrumb ${className}`.trim()}>
      <ol>
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const href = safeUrl(item.path);
          return (
            <li key={`${item.path}-${index}`}>
              {isLast || !href ? <span aria-current={isLast ? 'page' : undefined}>{item.title}</span> : <a href={href}>{item.title}</a>}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
