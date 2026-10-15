import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import userEvent from '@testing-library/user-event';
import { Button, ButtonLink, buttonClassName } from './Button';

describe('Button', () => {
  it('renders children and applies the tone class', () => {
    render(<Button tone="secondary">Gönder</Button>);
    const button = screen.getByRole('button', { name: 'Gönder' });
    expect(button).toHaveClass('button--secondary');
  });

  it('fires onClick and respects disabled state', async () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick} disabled>Gönder</Button>);
    await userEvent.click(screen.getByRole('button', { name: 'Gönder' }));
    expect(onClick).not.toHaveBeenCalled();
  });
});

describe('ButtonLink', () => {
  it('renders an anchor with the href', () => {
    render(<ButtonLink href="/iletisim">İletişim</ButtonLink>);
    expect(screen.getByRole('link', { name: 'İletişim' })).toHaveAttribute('href', '/iletisim');
  });
});

describe('buttonClassName', () => {
  it('builds the expected class list', () => {
    expect(buttonClassName('danger', true)).toBe('button button--danger button--block');
  });
});
