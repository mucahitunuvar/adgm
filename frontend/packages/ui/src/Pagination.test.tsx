import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Pagination } from './Pagination';

describe('Pagination', () => {
  it('renders nothing when everything fits on one page', () => {
    const { container } = render(<Pagination page={1} pageSize={20} totalCount={5} onPageChange={vi.fn()} />);
    expect(container).toBeEmptyDOMElement();
  });

  it('shows the summary and disables the previous button on the first page', async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={1} pageSize={20} totalCount={45} onPageChange={onPageChange} />);
    expect(screen.getByText('45 kayıttan 1–20')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Önceki sayfa' })).toBeDisabled();
    await userEvent.click(screen.getByRole('button', { name: 'Sonraki sayfa' }));
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it('disables the next button on the last page', () => {
    render(<Pagination page={3} pageSize={20} totalCount={45} onPageChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: 'Sonraki sayfa' })).toBeDisabled();
  });
});
