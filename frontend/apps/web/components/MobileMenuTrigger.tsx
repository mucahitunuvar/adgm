'use client';

import { useState } from 'react';
import { Menu as MenuIcon } from 'lucide-react';
import { AccessibleDialog, safeUrl } from '@gm/ui';
import type { PublicMenuItemResponse } from '@gm/api-client';
import { ThemeToggle } from './ThemeToggle';

interface MobileMenuTriggerProps {
  items: readonly PublicMenuItemResponse[];
  giveLoginHref: string;
  registerHref: string;
}

/**
 * FRONTEND-FAZ3-MASTER-PROMPT.md §2B: tam ekran çekmece, akordeon, Esc ile kapanır, giriş CTA'ları
 * altta sabit. Odak tuzağı/Esc/kaydırma kilidi `AccessibleDialog`'tan (packages/ui) gelir - burada
 * yeniden yazılmaz; akordeon yerel `<details>` ile (ekstra durum/JS gerekmez, klavye ile açılır).
 */
export function MobileMenuTrigger({ items, giveLoginHref, registerHref }: MobileMenuTriggerProps) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button type="button" className="icon-button mobile-menu-trigger" aria-label="Menüyü aç" onClick={() => setOpen(true)}>
        <MenuIcon aria-hidden="true" />
      </button>
      <AccessibleDialog open={open} onClose={() => setOpen(false)} title="Ana menü" className="mobile-menu">
        <nav aria-label="Ana menü" className="mobile-menu__nav">
          <ul className="mobile-menu__list">
            {items.map((item, index) => (
              <MobileMenuItem key={`${item.label}-${index}`} item={item} />
            ))}
          </ul>
        </nav>
        <div className="mobile-menu__cta">
          <ThemeToggle />
          <a className="button button--secondary button--block" href={giveLoginHref}>
            Giriş Yap
          </a>
          <a className="button button--primary button--block" href={registerHref}>
            Üye Ol
          </a>
        </div>
      </AccessibleDialog>
    </>
  );
}

function MobileMenuItem({ item }: { item: PublicMenuItemResponse }) {
  const href = safeUrl(item.href);
  if (item.children.length === 0) {
    return (
      <li>
        {href ? (
          <a href={href} target={item.openInNewTab ? '_blank' : undefined} rel={item.openInNewTab ? 'noopener noreferrer' : undefined}>
            {item.label}
          </a>
        ) : (
          <span className="mobile-menu__group-label">{item.label}</span>
        )}
      </li>
    );
  }

  return (
    <li>
      <details className="mobile-menu__details">
        <summary>{item.label}</summary>
        <ul>
          {item.children.map((child, index) => (
            <MobileMenuItem key={`${child.label}-${index}`} item={child} />
          ))}
        </ul>
      </details>
    </li>
  );
}
