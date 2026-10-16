import { safeUrl } from '@gm/ui';
import { iconKeyToLucide } from './iconKeyToLucide';

interface QuickLinkItem {
  iconKey?: string | null;
  label: string;
  description?: string | null;
  href: string | null;
}

export function QuickLinksBlock({ data }: { data: QuickLinkItem[] | null }) {
  if (!data || data.length === 0) return null;

  return (
    <section className="container section block-quick-links">
      <ul className="block-quick-links__grid">
        {data.map((item, index) => {
          const href = safeUrl(item.href);
          if (!href) return null;
          const Icon = iconKeyToLucide(item.iconKey);
          return (
            <li key={index}>
              <a href={href} className="block-quick-links__card">
                <Icon aria-hidden="true" />
                <span>{item.label}</span>
                {item.description && <small>{item.description}</small>}
              </a>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
