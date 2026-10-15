import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Tabs } from './Tabs';

const items = [
  { key: 'profile', label: 'Profil', content: <p>Profil içeriği</p> },
  { key: 'password', label: 'Parola', content: <p>Parola içeriği</p> },
];

describe('Tabs', () => {
  it('shows only the active panel and marks the active tab', () => {
    render(<Tabs items={items} activeKey="profile" onChange={vi.fn()} />);
    expect(screen.getByRole('tab', { name: 'Profil' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByText('Profil içeriği')).toBeVisible();
    expect(screen.queryByText('Parola içeriği')).not.toBeInTheDocument();
    expect(document.getElementById('tabpanel-password')).toHaveAttribute('hidden');
  });

  it('fires onChange when a tab is clicked', async () => {
    const onChange = vi.fn();
    render(<Tabs items={items} activeKey="profile" onChange={onChange} />);
    await userEvent.click(screen.getByRole('tab', { name: 'Parola' }));
    expect(onChange).toHaveBeenCalledWith('password');
  });

  it('moves focus and selection with the arrow keys', async () => {
    const onChange = vi.fn();
    render(<Tabs items={items} activeKey="profile" onChange={onChange} />);
    screen.getByRole('tab', { name: 'Profil' }).focus();
    await userEvent.keyboard('{ArrowRight}');
    expect(onChange).toHaveBeenCalledWith('password');
  });
});
