import { safeUrl } from '@gm/ui';
import type { PublicMenuItemResponse } from '@gm/api-client';

/**
 * Masaüstü ana menü - iç içe öğeler `:focus-within`/`:hover` ile açılır (CSS-only), klavye ile Tab
 * sırasıyla erişilebilir, ekstra JS/durum gerekmez (ADR-024 §7: menü derinliği en fazla 3 - bu
 * yüzden `NavItem` kendi çocuklarını yine `NavItem` ile basar, derinlik backend'in verdiği kadar).
 * `href` backend'den zaten çözülmüş gelir (bkz. BLOK-ESLEMESI.md madde v); `safeUrl` yine de
 * savunma amaçlı uygulanır (FRONTEND-GUVENLIK.md §3 - içerik kaynaklı her href doğrulanır).
 */
export function DesktopNav({ items }: { items: readonly PublicMenuItemResponse[] }) {
  if (items.length === 0) return null;
  return (
    <nav aria-label="Ana menü" className="desktop-nav">
      <ul className="desktop-nav__list">
        {items.map((item, index) => (
          <NavItem key={`${item.label}-${index}`} item={item} depth={0} />
        ))}
      </ul>
    </nav>
  );
}

function NavItem({ item, depth }: { item: PublicMenuItemResponse; depth: number }) {
  const href = safeUrl(item.href);
  const hasChildren = item.children.length > 0;

  return (
    <li className={`desktop-nav__item desktop-nav__item--depth-${depth}`}>
      {href ? (
        <a href={href} target={item.openInNewTab ? '_blank' : undefined} rel={item.openInNewTab ? 'noopener noreferrer' : undefined}>
          {item.label}
        </a>
      ) : (
        <span className="desktop-nav__group-label">{item.label}</span>
      )}
      {hasChildren && (
        <ul className="desktop-nav__submenu">
          {item.children.map((child, index) => (
            <NavItem key={`${child.label}-${index}`} item={child} depth={depth + 1} />
          ))}
        </ul>
      )}
    </li>
  );
}
