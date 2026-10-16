import { BlockImage } from '../blocks/BlockImage';
import type { PublicContentDetailResponse } from '@gm/api-client';

export function GallerySection({ gallery }: { gallery: PublicContentDetailResponse['gallery'] }) {
  if (gallery.length === 0) return null;
  return (
    <section className="content-detail__section">
      <h2>Galeri</h2>
      <ul className="block-gallery__grid">
        {gallery.map((item) => (
          <li key={item.mediaAssetId}>
            <BlockImage image={item.image} alt={item.altText} sizes="(min-width: 900px) 25vw, 50vw" />
            {item.caption && <p className="content-detail__caption">{item.caption}</p>}
          </li>
        ))}
      </ul>
    </section>
  );
}
