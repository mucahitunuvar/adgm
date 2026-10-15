import { ArrowDown, ArrowUp } from 'lucide-react';
import type { ReactNode } from 'react';
import { EmptyState, ErrorState, LoadingState } from './PageState';

export interface DataTableColumn<TRow> {
  key: string;
  header: string;
  render: (row: TRow) => ReactNode;
  sortable?: boolean;
  align?: 'start' | 'end';
}

export interface DataTableProps<TRow> {
  caption: string;
  columns: DataTableColumn<TRow>[];
  rows: TRow[];
  getRowKey: (row: TRow) => string;
  isLoading?: boolean;
  error?: string;
  emptyTitle?: string;
  emptyDescription?: string;
  sortKey?: string;
  sortDirection?: 'asc' | 'desc';
  onSortChange?: (key: string) => void;
  rowActions?: (row: TRow) => ReactNode;
}

/** Sıralanabilir, satır eylemli, erişilebilir veri tablosu; yükleniyor/boş/hata durumlarını kendi render eder. */
export function DataTable<TRow>({
  caption,
  columns,
  rows,
  getRowKey,
  isLoading = false,
  error,
  emptyTitle = 'Kayıt bulunamadı',
  emptyDescription = 'Bu filtrelerle eşleşen bir kayıt yok.',
  sortKey,
  sortDirection,
  onSortChange,
  rowActions,
}: DataTableProps<TRow>) {
  if (isLoading) return <LoadingState />;
  if (error) return <ErrorState description={error} />;
  if (rows.length === 0) return <EmptyState title={emptyTitle} description={emptyDescription} />;

  return (
    <div className="data-table-wrap">
      <table className="data-table">
        <caption className="sr-only">{caption}</caption>
        <thead>
          <tr>
            {columns.map((column) => {
              const ariaSort = column.sortable && sortKey === column.key
                ? (sortDirection === 'desc' ? 'descending' : 'ascending')
                : column.sortable
                  ? 'none'
                  : undefined;
              return (
                <th key={column.key} scope="col" aria-sort={ariaSort} data-align={column.align}>
                  {column.sortable && onSortChange ? (
                    <button
                      type="button"
                      className="data-table__sort"
                      onClick={() => onSortChange(column.key)}
                    >
                      {column.header}
                      {sortKey === column.key && (sortDirection === 'desc' ? <ArrowDown aria-hidden="true" /> : <ArrowUp aria-hidden="true" />)}
                    </button>
                  ) : (
                    column.header
                  )}
                </th>
              );
            })}
            {rowActions && (
              <th scope="col">
                <span className="sr-only">İşlemler</span>
              </th>
            )}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={getRowKey(row)}>
              {columns.map((column) => (
                <td key={column.key} data-align={column.align}>
                  {column.render(row)}
                </td>
              ))}
              {rowActions && <td className="data-table__actions">{rowActions(row)}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
