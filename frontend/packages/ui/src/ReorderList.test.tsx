import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ReorderList } from './ReorderList';

interface Item {
  id: string;
  name: string;
}

const items: Item[] = [
  { id: '1', name: 'Ana Sayfa' },
  { id: '2', name: 'Hakkımızda' },
  { id: '3', name: 'İletişim' },
];

describe('ReorderList', () => {
  it('disables "yukarı taşı" on the first item and "aşağı taşı" on the last', () => {
    render(<ReorderList items={items} getKey={(i) => i.id} getLabel={(i) => i.name} renderItem={(i) => i.name} onMove={vi.fn()} />);

    expect(screen.getByRole('button', { name: 'Ana Sayfa öğesini yukarı taşı' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'İletişim öğesini aşağı taşı' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Ana Sayfa öğesini aşağı taşı' })).toBeEnabled();
  });

  it('calls onMove with the from/to indexes and announces the new position', async () => {
    const onMove = vi.fn();
    render(<ReorderList items={items} getKey={(i) => i.id} getLabel={(i) => i.name} renderItem={(i) => i.name} onMove={onMove} />);

    await userEvent.click(screen.getByRole('button', { name: 'Hakkımızda öğesini yukarı taşı' }));

    expect(onMove).toHaveBeenCalledWith(1, 0);
    expect(screen.getByRole('status')).toHaveTextContent('Hakkımızda, 1. sıraya taşındı.');
  });

  it('disables every button when disabled is true', () => {
    render(
      <ReorderList items={items} getKey={(i) => i.id} getLabel={(i) => i.name} renderItem={(i) => i.name} onMove={vi.fn()} disabled />,
    );

    for (const button of screen.getAllByRole('button')) {
      expect(button).toBeDisabled();
    }
  });
});
