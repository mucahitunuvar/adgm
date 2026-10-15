import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { DataTable, type DataTableColumn } from './DataTable';

interface Row {
  id: string;
  name: string;
}

const columns: DataTableColumn<Row>[] = [
  { key: 'name', header: 'Ad', render: (row) => row.name, sortable: true },
];

describe('DataTable', () => {
  it('shows the loading state', () => {
    render(<DataTable caption="Kullanıcılar" columns={columns} rows={[]} getRowKey={(r) => r.id} isLoading />);
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('shows the error state', () => {
    render(<DataTable caption="Kullanıcılar" columns={columns} rows={[]} getRowKey={(r) => r.id} error="Sunucu hatası" />);
    expect(screen.getByRole('alert')).toHaveTextContent('Sunucu hatası');
  });

  it('shows the empty state when there are no rows', () => {
    render(<DataTable caption="Kullanıcılar" columns={columns} rows={[]} getRowKey={(r) => r.id} />);
    expect(screen.getByText('Kayıt bulunamadı')).toBeInTheDocument();
  });

  it('renders rows and fires onSortChange from the header button', async () => {
    const onSortChange = vi.fn();
    render(
      <DataTable
        caption="Kullanıcılar"
        columns={columns}
        rows={[{ id: '1', name: 'Ayşe' }]}
        getRowKey={(r) => r.id}
        sortKey="name"
        sortDirection="asc"
        onSortChange={onSortChange}
      />,
    );
    expect(screen.getByText('Ayşe')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: /Ad/ }));
    expect(onSortChange).toHaveBeenCalledWith('name');
  });

  it('renders row actions', () => {
    render(
      <DataTable
        caption="Kullanıcılar"
        columns={columns}
        rows={[{ id: '1', name: 'Ayşe' }]}
        getRowKey={(r) => r.id}
        rowActions={(row) => <button type="button">{`Düzenle ${row.name}`}</button>}
      />,
    );
    expect(screen.getByRole('button', { name: 'Düzenle Ayşe' })).toBeInTheDocument();
  });
});
