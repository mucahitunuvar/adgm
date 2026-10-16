'use client';

import type { FormEvent, ReactNode } from 'react';
import { Button } from './Button';

export interface FilterBarProps {
  children: ReactNode;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onReset?: () => void;
  submitLabel?: string;
  resetLabel?: string;
}

/** Liste filtreleri için form kabuğu; arama rolü ve klavye ile gönderim içerir. */
export function FilterBar({ children, onSubmit, onReset, submitLabel = 'Filtrele', resetLabel = 'Temizle' }: FilterBarProps) {
  return (
    <form className="filter-bar" role="search" onSubmit={onSubmit}>
      <div className="filter-bar__fields">{children}</div>
      <div className="filter-bar__actions">
        <Button type="submit" tone="secondary">
          {submitLabel}
        </Button>
        {onReset && (
          <Button type="button" tone="quiet" onClick={onReset}>
            {resetLabel}
          </Button>
        )}
      </div>
    </form>
  );
}
