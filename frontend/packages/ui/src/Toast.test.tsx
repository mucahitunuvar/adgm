import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { ToastProvider, useToast } from './Toast';

function Trigger() {
  const { showToast } = useToast();
  return (
    <button type="button" onClick={() => showToast('Kullanıcı devre dışı bırakıldı', 'success')}>
      Göster
    </button>
  );
}

describe('ToastProvider / useToast', () => {
  it('shows a toast and dismisses it on click', async () => {
    render(
      <ToastProvider>
        <Trigger />
      </ToastProvider>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Göster' }));
    expect(screen.getByText('Kullanıcı devre dışı bırakıldı')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Bildirimi kapat' }));
    expect(screen.queryByText('Kullanıcı devre dışı bırakıldı')).not.toBeInTheDocument();
  });

  it('throws when useToast is used outside the provider', () => {
    function Lonely() {
      useToast();
      return null;
    }
    expect(() => render(<Lonely />)).toThrow('useToast, ToastProvider içinde kullanılmalıdır.');
  });
});
