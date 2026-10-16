import { Brand, safeUrl } from '@gm/ui';
import type { PublicSiteResponse } from '@gm/api-client';
import { DesktopNav } from './DesktopNav';
import { MobileMenuTrigger } from './MobileMenuTrigger';
import { ThemeToggle } from './ThemeToggle';

const LOGIN_HREF = '/giris';
const REGISTER_HREF = '/uye-ol';

/**
 * Sticky header (FRONTEND-FAZ3-MASTER-PROMPT.md §2B). `Utility` konumu varsa (çoğunlukla boş)
 * üstte ince bir şerit olarak basılır; `Header` menüsü ana navigasyon.
 */
export function Header({ site }: { site: PublicSiteResponse }) {
  const logoLight = safeUrl(site.logoLightUrl);
  const logoDark = safeUrl(site.logoDarkUrl);

  return (
    <header className="site-header">
      {site.menus.utility.length > 0 && (
        <div className="site-header__utility">
          <div className="container site-header__utility-inner">
            <nav aria-label="Yardımcı bağlantılar">
              <ul className="site-header__utility-list">
                {site.menus.utility.map((item, index) => {
                  const href = safeUrl(item.href);
                  return (
                    <li key={`${item.label}-${index}`}>
                      {href ? (
                        <a href={href} target={item.openInNewTab ? '_blank' : undefined} rel={item.openInNewTab ? 'noopener noreferrer' : undefined}>
                          {item.label}
                        </a>
                      ) : (
                        item.label
                      )}
                    </li>
                  );
                })}
              </ul>
            </nav>
          </div>
        </div>
      )}

      <div className="container site-header__row">
        <a href="/" className="brand" aria-label={site.siteName || 'Ana sayfa'}>
          {logoLight || logoDark ? (
            <>
              {logoLight && <img src={logoLight} alt={site.siteName} className="site-header__logo site-header__logo--light" />}
              {logoDark && <img src={logoDark} alt={site.siteName} className="site-header__logo site-header__logo--dark" />}
            </>
          ) : (
            <Brand />
          )}
        </a>

        <DesktopNav items={site.menus.header} />

        <form action="/ilanlar" method="get" className="site-header__search" role="search">
          <label htmlFor="site-search-q" className="sr-only">
            İlan ara
          </label>
          <input id="site-search-q" name="q" type="search" placeholder="İlan ara" minLength={2} maxLength={100} />
        </form>

        <div className="site-header__actions">
          <div className="site-header__theme-desktop">
            <ThemeToggle />
          </div>
          <a className="button button--secondary site-header__login" href={LOGIN_HREF}>
            Giriş Yap
          </a>
          <a className="button button--primary site-header__register" href={REGISTER_HREF}>
            Üye Ol
          </a>
          <div className="site-header__mobile-trigger">
            <MobileMenuTrigger items={site.menus.header} giveLoginHref={LOGIN_HREF} registerHref={REGISTER_HREF} />
          </div>
        </div>
      </div>
    </header>
  );
}
