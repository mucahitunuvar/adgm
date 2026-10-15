import { AuthProvider } from '@gm/auth';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { LoginPage } from './LoginPage';

function problemResponse(title: string, status: number, detail?: string) {
  return new Response(JSON.stringify({ title, status, detail }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  });
}

describe('LoginPage', () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  function renderPage(initialPath = '/giris') {
    return render(
      <MemoryRouter initialEntries={[initialPath]}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/giris" element={<LoginPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );
  }

  it('shows a generic error for invalid credentials (anti-enumeration), without writing anything to storage', async () => {
    // Yanlış parola ile yanlış hesap aynı mesajı taşımalı; client 401'de bir kez yeniden
    // dener (onUnauthorized -> refresh, token yok -> hemen false), bu yüzden aynı yanıtı her
    // çağrıda döneriz.
    fetchMock.mockResolvedValue(problemResponse('Auth.InvalidCredentials', 401, 'Invalid email or password.'));

    const { container } = renderPage();
    await userEvent.type(screen.getByLabelText(/^E-posta/), 'wrong@example.com');
    await userEvent.type(container.querySelector('input[type="password"]')!, 'wrongpassword1');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş yap' }));

    expect(await screen.findByText('E-posta veya parola hatalı.')).toBeInTheDocument();
    expect(localStorage.length).toBe(0);
    expect(sessionStorage.length).toBe(0);
  });

  it('shows the account-locked message distinctly from invalid credentials', async () => {
    fetchMock.mockResolvedValue(
      problemResponse('Auth.AccountLocked', 403, 'This account is temporarily locked until 2026-01-01T00:00:00Z.'),
    );

    const { container } = renderPage();
    await userEvent.type(screen.getByLabelText(/^E-posta/), 'locked@example.com');
    await userEvent.type(container.querySelector('input[type="password"]')!, 'somepassword1');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş yap' }));

    expect(
      await screen.findByText(/hesabınız geçici olarak kilitlendi/),
    ).toBeInTheDocument();
  });

  it('shows a "session expired" notice when redirected here after a protected route', () => {
    render(
      <MemoryRouter initialEntries={[{ pathname: '/giris', state: { from: '/hesabim' } }]}>
        <AuthProvider baseUrl="http://test.local">
          <Routes>
            <Route path="/giris" element={<LoginPage />} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>,
    );

    expect(screen.getByText('Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.')).toBeInTheDocument();
  });

  it('shows a rate-limit message with the Retry-After duration on 429', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 429, headers: { 'Retry-After': '20' } }));

    const { container } = renderPage();
    await userEvent.type(screen.getByLabelText(/^E-posta/), 'someone@example.com');
    await userEvent.type(container.querySelector('input[type="password"]')!, 'somepassword1');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş yap' }));

    expect(await screen.findByText('Çok sık denediniz. Lütfen 20 saniye sonra tekrar deneyin.')).toBeInTheDocument();
  });
});
