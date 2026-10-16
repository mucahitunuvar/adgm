import { safeUrl } from '@gm/ui';
import type { PublicContentCategoryTreeItemResponse } from '@gm/api-client';

interface CategoryFilterNavProps {
  basePath: string;
  categories: readonly PublicContentCategoryTreeItemResponse[];
  selectedSlug?: string | null;
  search?: string;
}

/** Kategori bağlantıları düz `<a>`dır (JS gerektirmez, taranabilir); seçim değişince sayfalama sıfırlanır. */
export function CategoryFilterNav({ basePath, categories, selectedSlug, search }: CategoryFilterNavProps) {
  if (categories.length === 0) return null;

  function hrefFor(slug?: string): string | undefined {
    const params = new URLSearchParams();
    if (slug) params.set('category', slug);
    if (search) params.set('search', search);
    const query = params.toString();
    return safeUrl(query ? `${basePath}?${query}` : basePath);
  }

  return (
    <nav aria-label="Kategoriler" className="category-filter-nav">
      <ul>
        <li>
          <a href={hrefFor()} aria-current={!selectedSlug ? 'true' : undefined}>
            Tümü
          </a>
        </li>
        {categories.map((category) => (
          <li key={category.id}>
            <a href={hrefFor(category.slug)} aria-current={selectedSlug === category.slug ? 'true' : undefined}>
              {category.name}
            </a>
            {category.children.length > 0 && (
              <ul>
                {category.children.map((child) => (
                  <li key={child.id}>
                    <a href={hrefFor(child.slug)} aria-current={selectedSlug === child.slug ? 'true' : undefined}>
                      {child.name}
                    </a>
                  </li>
                ))}
              </ul>
            )}
          </li>
        ))}
      </ul>
    </nav>
  );
}
