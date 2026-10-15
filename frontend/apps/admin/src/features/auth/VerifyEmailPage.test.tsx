import { AuthProvider } from '@gm/auth';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { VerifyEmailPage } from './VerifyEmailPage';

function okResponse() {
  return new Response(null, { status: 204 });
}

describe('VerifyEmailPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(okResponse()));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    window.history.replaceState(null, '', '/');
  });

  it('reads the token once, strips it from the address bar, and confirms it over POST (not GET)', async () => {
    const path = '/eposta-dogrulama?token=super-secret-verification-token';
    window.history.pushState(null, '', path);

    render(
      <MemoryRouter initialEntries={[path]}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/eposta-dogrulama" element={<VerifyEmailPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );

    expect(window.location.search).toBe('');
    expect(window.location.href).not.toContain('super-secret-verification-token');
    expect(await screen.findByText('E-posta adresiniz doğrulandı.')).toBeInTheDocument();

    const fetchMock = vi.mocked(fetch);
    const verifyCall = fetchMock.mock.calls.find(([, init]) => (init as RequestInit | undefined)?.method === 'POST');
    expect(verifyCall).toBeDefined();
  });

  it('shows an error when there is no token at all', () => {
    render(
      <MemoryRouter initialEntries={['/eposta-dogrulama']}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/eposta-dogrulama" element={<VerifyEmailPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );

    expect(screen.getByText('Doğrulama bağlantısı geçersiz.')).toBeInTheDocument();
  });
});
