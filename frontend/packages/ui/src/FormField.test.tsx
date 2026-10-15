import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { TextField } from './FormField';

describe('TextField', () => {
  it('associates label and error with the input', () => {
    render(<TextField id="email" label="E-posta" error="Geçerli bir adres yazın." required />);
    const input = screen.getByLabelText(/E-posta/);
    expect(input).toHaveAttribute('aria-invalid', 'true');
    expect(input).toHaveAccessibleDescription('Geçerli bir adres yazın.');
  });

  it('marks optional fields in the label when not required', () => {
    render(<TextField id="phone" label="Telefon" />);
    expect(screen.getByText(/isteğe bağlı/)).toBeInTheDocument();
  });
});
