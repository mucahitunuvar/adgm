import { SafeHtml } from '@gm/ui';
import type { PublicContentListItemResponse } from '@gm/api-client';

/** `faq-accordion` şablonu: `HasDetailPage=false`, gövde (cevap) listede satır içi döner. */
export function FaqAccordionList({ items }: { items: readonly PublicContentListItemResponse[] }) {
  return (
    <div className="block-faq__list">
      {items.map((item) => (
        <details key={item.id} className="block-faq__item">
          <summary>{item.title}</summary>
          {item.body && <SafeHtml html={item.body} />}
        </details>
      ))}
    </div>
  );
}
