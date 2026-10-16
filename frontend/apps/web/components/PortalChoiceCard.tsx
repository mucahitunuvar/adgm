interface PortalChoiceCardProps {
  title: string;
  description: string;
  href: string;
  ctaLabel: string;
}

/**
 * FRONTEND-FAZ3-MASTER-PROMPT.md §2B: solda Aday, sağda Firma kartı (mobilde alt alta); public
 * sitede şifre alanı yok - bu yalnız ilgili portala yönlendiren bir karttır.
 */
export function PortalChoiceCard({ title, description, href, ctaLabel }: PortalChoiceCardProps) {
  return (
    <div className="portal-choice-card">
      <h2>{title}</h2>
      <p>{description}</p>
      <a className="button button--primary" href={href}>
        {ctaLabel}
      </a>
    </div>
  );
}
