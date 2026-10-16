import { BlockImage } from './BlockImage';

interface GalleryImage {
  id: string;
  image: { small?: string | null; medium?: string | null; large?: string | null; original: string };
  altText?: string | null;
}

export function GalleryBlock({ texts, data }: { texts: { title?: string } | null; data: GalleryImage[] | null }) {
  if (!data || data.length === 0) return null;

  return (
    <section className="container section block-gallery">
      {texts?.title && <h2>{texts.title}</h2>}
      <ul className="block-gallery__grid">
        {data.map((item) => (
          <li key={item.id}>
            <BlockImage image={item.image} alt={item.altText ?? ''} sizes="(min-width: 900px) 25vw, 50vw" />
          </li>
        ))}
      </ul>
    </section>
  );
}
