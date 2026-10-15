import type { AnchorHTMLAttributes, ButtonHTMLAttributes, ReactNode } from 'react';

export type ButtonTone = 'primary' | 'secondary' | 'quiet' | 'danger';

interface CommonProps {
  children: ReactNode;
  tone?: ButtonTone;
  block?: boolean;
}

export function Button({
  children,
  tone = 'primary',
  block = false,
  className = '',
  ...props
}: CommonProps & ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button className={`button button--${tone}${block ? ' button--block' : ''} ${className}`.trim()} {...props}>
      {children}
    </button>
  );
}

/**
 * Yönlendirme kütüphanesinden bağımsızdır (Next ve Vite'ta ortak kullanılır).
 * Uygulama içi gezinme gereken yerlerde çağıran taraf kendi `Link` bileşenini
 * `asChild` deseniyle sarmalı değil; bunun yerine `className` üretimi için
 * `buttonClassName` yardımcısını kullanabilir.
 */
export function ButtonLink({
  children,
  tone = 'primary',
  block = false,
  className = '',
  ...props
}: CommonProps & AnchorHTMLAttributes<HTMLAnchorElement>) {
  return (
    <a className={`button button--${tone}${block ? ' button--block' : ''} ${className}`.trim()} {...props}>
      {children}
    </a>
  );
}

export function buttonClassName(tone: ButtonTone = 'primary', block = false): string {
  return `button button--${tone}${block ? ' button--block' : ''}`;
}
