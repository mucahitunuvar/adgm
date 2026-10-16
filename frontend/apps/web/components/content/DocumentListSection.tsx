import { FileText } from 'lucide-react';
import { safeUrl } from '@gm/ui';
import type { PublicContentListItemResponse } from '@gm/api-client';

/** `document-list` şablonu (Dökümanlar): `HasDetailPage=false`, her öğenin kendi ekleri indirilebilir liste olarak basılır. */
export function DocumentListSection({ items }: { items: readonly PublicContentListItemResponse[] }) {
  return (
    <ul className="document-list">
      {items.map((item) => (
        <li key={item.id} className="document-list__item">
          <div>
            <h3>{item.title}</h3>
            {item.summary && <p>{item.summary}</p>}
          </div>
          {item.attachments && item.attachments.length > 0 && (
            <ul className="document-list__files">
              {item.attachments.map((attachment, index) => {
                const href = safeUrl(attachment.url);
                if (!href) return null;
                return (
                  <li key={`${attachment.name}-${index}`}>
                    <a href={href} download>
                      <FileText aria-hidden="true" />
                      <span>{attachment.name}</span>
                    </a>
                  </li>
                );
              })}
            </ul>
          )}
        </li>
      ))}
    </ul>
  );
}
