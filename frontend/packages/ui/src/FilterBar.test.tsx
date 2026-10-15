import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { FormEvent } from 'react';
import { describe, expect, it, vi } from 'vitest';
import { FilterBar } from './FilterBar';

describe('FilterBar', () => {
  it('fires onSubmit and onReset', async () => {
    const onSubmit = vi.fn((event: FormEvent<HTMLFormElement>) => event.preventDefault());
    const onReset = vi.fn();
    render(
      <FilterBar onSubmit={onSubmit} onReset={onReset}>
        <label htmlFor="q">Ara</label>
        <input id="q" />
      </FilterBar>,
    );

    expect(screen.getByRole('search')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Filtrele' }));
    expect(onSubmit).toHaveBeenCalledTimes(1);
    await userEvent.click(screen.getByRole('button', { name: 'Temizle' }));
    expect(onReset).toHaveBeenCalledTimes(1);
  });

  it('omits the reset button when onReset is not provided', () => {
    render(
      <FilterBar onSubmit={vi.fn()}>
        <input aria-label="Ara" />
      </FilterBar>,
    );
    expect(screen.queryByRole('button', { name: 'Temizle' })).not.toBeInTheDocument();
  });
});
