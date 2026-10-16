'use client';

import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from './Button';

export interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

/** Sayfalama kontrolleri; `totalCount` sayfaya sığıyorsa hiçbir şey render etmez. */
export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (totalPages <= 1) return null;

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  return (
    <nav className="pagination" aria-label="Sayfalama">
      <p className="pagination__summary">
        {totalCount} kayıttan {from}–{to}
      </p>
      <div className="pagination__controls">
        <Button
          type="button"
          tone="quiet"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          aria-label="Önceki sayfa"
        >
          <ChevronLeft aria-hidden="true" />
        </Button>
        <span className="pagination__page">
          {page} / {totalPages}
        </span>
        <Button
          type="button"
          tone="quiet"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          aria-label="Sonraki sayfa"
        >
          <ChevronRight aria-hidden="true" />
        </Button>
      </div>
    </nav>
  );
}
