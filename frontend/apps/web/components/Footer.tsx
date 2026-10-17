import { Brand, safeUrl } from '@gm/ui';
import type { PublicSiteResponse } from '@gm/api-client';
import { CookiePreferencesButton } from './CookiePreferencesButton';
import { NewsletterSection } from './NewsletterSection';

/**
 * FRONTEND-FAZ3-MASTER-PROMPT.md §2B: 4 kolon + alt şerit. Backend `Footer` menüsünü grup
 * başlıklarıyla (çocuksuz link = grup değil, tek bağlantı) tutar; kaç grup geldiği admin'in menü
 * kurgusuna bağlıdır - burada sabit "Kurumsal"/"İş ve Kariyer" etiketi varsayılmaz, gelen her grup
 * kendi başlığıyla bir kolon olur (ortak ilke: boş kolon çizilmez).
 */
export function Footer({ site }: { site: PublicSiteResponse }) {
  const groups = site.menus.footer.filter((item) => item.children.length > 0);
  const standaloneLinks = site.menus.footer.filter((item) => item.children.length === 0);
  const year = new Date().getFullYear();

  return (
    <footer className="site-footer">
      <div className="container">
        <NewsletterSection enabled={site.newsletterEnabled} privacyNotice={site.newsletterPrivacyNotice} />
      </div>
      <div className="container site-footer__grid">
        <div className="site-footer__brand">
          <Brand />
          {site.footerText && <p>{site.footerText}</p>}
          {site.socialLinks.length > 0 && (
            <ul className="site-footer__social">
              {site.socialLinks.map((link, index) => {
                const href = safeUrl(link.url);
                return href ? (
                  <li key={index}>
                    <a href={href} target="_blank" rel="noopener noreferrer">
                      {link.platform}
                    </a>
                  </li>
                ) : null;
              })}
            </ul>
          )}
        </div>

        {groups.map((group, index) => (
          <div className="site-footer__column" key={`${group.label}-${index}`}>
            <h2>{group.label}</h2>
            <ul>
              {group.children.map((child, childIndex) => {
                const href = safeUrl(child.href);
                return href ? (
                  <li key={childIndex}>
                    <a href={href} target={child.openInNewTab ? '_blank' : undefined} rel={child.openInNewTab ? 'noopener noreferrer' : undefined}>
                      {child.label}
                    </a>
                  </li>
                ) : null;
              })}
            </ul>
          </div>
        ))}

        {standaloneLinks.length > 0 && (
          <div className="site-footer__column">
            <h2 className="sr-only">Bağlantılar</h2>
            <ul>
              {standaloneLinks.map((link, index) => {
                const href = safeUrl(link.href);
                return href ? (
                  <li key={index}>
                    <a href={href} target={link.openInNewTab ? '_blank' : undefined} rel={link.openInNewTab ? 'noopener noreferrer' : undefined}>
                      {link.label}
                    </a>
                  </li>
                ) : null;
              })}
            </ul>
          </div>
        )}

        {(site.contact.address || site.contact.phone || site.contact.email) && (
          <div className="site-footer__column">
            <h2>İletişim</h2>
            <ul className="site-footer__contact">
              {site.contact.phone && (
                <li>
                  <a href={`tel:${site.contact.phone}`}>{site.contact.phone}</a>
                </li>
              )}
              {site.contact.email && (
                <li>
                  <a href={`mailto:${site.contact.email}`}>{site.contact.email}</a>
                </li>
              )}
              {site.contact.address && <li>{site.contact.address}</li>}
            </ul>
          </div>
        )}
      </div>

      <div className="container site-footer__bottom">
        <CookiePreferencesButton />
        <p>
          © {year} {site.siteName || 'Ahmet Demir Gençlik Merkezi'}
        </p>
        <p>Design &amp; Software: Alaz Karbon</p>
      </div>
    </footer>
  );
}
