import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { PasswordField } from './PasswordField';

describe('PasswordField', () => {
  it('starts masked and toggles to visible', async () => {
    const { container } = render(<PasswordField id="password" label="Parola" required />);
    const input = container.querySelector('#password') as HTMLInputElement;
    expect(input).toHaveAttribute('type', 'password');

    await userEvent.click(screen.getByRole('button', { name: 'Parolayı göster' }));
    expect(input).toHaveAttribute('type', 'text');

    await userEvent.click(screen.getByRole('button', { name: 'Parolayı gizle' }));
    expect(input).toHaveAttribute('type', 'password');
  });

  it('shows the error message and marks the input invalid', () => {
    const { container } = render(<PasswordField id="password" label="Parola" error="En az 8 karakter olmalı" />);
    expect(screen.getByText('En az 8 karakter olmalı')).toBeInTheDocument();
    expect(container.querySelector('#password')).toHaveAttribute('aria-invalid', 'true');
  });
});
