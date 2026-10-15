import { AuthProvider } from '@gm/auth';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ResetPasswordPage } from './ResetPasswordPage';

describe('ResetPasswordPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    window.history.replaceState(null, '', '/');
  });

  it('reads the token once and immediately strips it from the address bar', () => {
    const path = '/sifre-sifirla?token=super-secret-reset-token';
    window.history.pushState(null, '', path);

    render(
      <MemoryRouter initialEntries={[path]}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/sifre-sifirla" element={<ResetPasswordPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );

    expect(window.location.search).toBe('');
    expect(window.location.href).not.toContain('super-secret-reset-token');
    expect(screen.getByRole('button', { name: 'Parolayı sıfırla' })).toBeInTheDocument();
  });

  it('shows a validation error and never sends the request when the passwords do not match', async () => {
    const path = '/sifre-sifirla?token=abc';
    window.history.pushState(null, '', path);
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    const { container } = render(
      <MemoryRouter initialEntries={[path]}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/sifre-sifirla" element={<ResetPasswordPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );

    const inputs = container.querySelectorAll('input[type="password"]');
    await userEvent.type(inputs[0]!, 'NewPassword1');
    await userEvent.type(inputs[1]!, 'DifferentPassword1');
    await userEvent.click(screen.getByRole('button', { name: 'Parolayı sıfırla' }));

    expect(await screen.findByText('Parolalar eşleşmiyor.')).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
