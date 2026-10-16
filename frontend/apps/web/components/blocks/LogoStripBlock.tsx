import { safeUrl } from '@gm/ui';
import { BlockImage } from './BlockImage';

interface Partner {
  id: string;
  name: string;
  websiteUrl?: string | null;
  logo: { small?: string | null; medium?: string | null; large?: string | null; original: string };
}

export function LogoStripBlock({ texts, data }: { texts: { title?: string } | null; data: Partner[] | null }) {
  if (!data || data.length === 0) return null;

  return (
    <section className="container section block-logo-strip">
      {texts?.title && <h2>{texts.title}</h2>}
      <ul className="block-logo-strip__list">
        {data.map((partner) => {
          const href = safeUrl(partner.websiteUrl);
          const logo = <BlockImage image={partner.logo} alt={partner.name} sizes="200px" />;
          return (
            <li key={partner.id}>
              {href ? (
                <a href={href} target="_blank" rel="noopener noreferrer" aria-label={partner.name}>
                  {logo}
                </a>
              ) : (
                logo
              )}
            </li>
          );
        })}
      </ul>
    </section>
  );
}
