import { safeUrl } from '@gm/ui';
import { BlockImage } from '../blocks/BlockImage';
import type { PublicContentDetailResponse } from '@gm/api-client';

/** Hiyerarşik türlerde (`SupportsHierarchy`, ör. "Sayfa") alt sayfaların listesi. */
export function ChildrenSection({ children }: { children: PublicContentDetailResponse['children'] }) {
  if (children.length === 0) return null;
  return (
    <section className="content-detail__section">
      <h2>Alt sayfalar</h2>
      <div className="block-content-list__grid">
        {children.map((child) => {
          const href = safeUrl(child.path);
          const body = (
            <>
              {child.coverImage && <BlockImage image={child.coverImage} alt={child.title} sizes="(min-width: 900px) 33vw, 100vw" />}
              <h3>{child.title}</h3>
              <p>{child.summary}</p>
            </>
          );
          return href ? (
            <a key={child.id} href={href} className="content-card">
              {body}
            </a>
          ) : (
            <div key={child.id} className="content-card content-card--static">
              {body}
            </div>
          );
        })}
      </div>
    </section>
  );
}
