import { safeUrl } from '@gm/ui';

interface CtaButton {
  label: string;
  style: string;
  href: string | null;
}

interface CtaData {
  eyebrow?: string | null;
  title: string;
  buttons: CtaButton[];
}

const STYLE_CLASS: Record<string, string> = {
  primary: 'button--primary',
  secondary: 'button--secondary',
  quiet: 'button--quiet',
};

export function CtaBlock({ data }: { data: CtaData | null }) {
  if (!data) return null;

  return (
    <section className="container section block-cta">
      {data.eyebrow && <p className="eyebrow">{data.eyebrow}</p>}
      <h2>{data.title}</h2>
      <div className="block-cta__actions">
        {data.buttons.map((button, index) => {
          const href = safeUrl(button.href);
          if (!href) return null;
          return (
            <a key={index} href={href} className={`button ${STYLE_CLASS[button.style] ?? 'button--primary'}`}>
              {button.label}
            </a>
          );
        })}
      </div>
    </section>
  );
}
