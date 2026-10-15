import { cleanup, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProtectedRoute } from './ProtectedRoute';
import { useAuth } from './AuthProvider';

vi.mock('./AuthProvider', () => ({
  useAuth: vi.fn(),
}));

function SignInPage() {
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from;
  return <p>Giriş sayfası{from ? ` (from: ${from})` : ''}</p>;
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/giris" element={<SignInPage />} />
        <Route path="/yetkisiz" element={<p>Yetkisiz sayfası</p>} />
        <Route element={<ProtectedRoute roles={['Employer']} />}>
          <Route path="/portal/firma" element={<p>Korunan içerik</p>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

afterEach(() => {
  cleanup();
});

describe('ProtectedRoute', () => {
  it('redirects to the sign-in page when there is no session (e.g. after a 401)', () => {
    vi.mocked(useAuth).mockReturnValue({ user: null, isLoading: false } as ReturnType<typeof useAuth>);
    renderAt('/portal/firma');
    expect(screen.getByText(/^Giriş sayfası/)).toBeInTheDocument();
  });

  // Paylaşılan bir filtre/sayfa URL'si (ör. ?categoryId=...) login akışından sonra kaybolmamalı -
  // `from` yalnız `pathname` değil, sorgu dizesini de taşımalı.
  it('preserves the query string in the redirect state so a shared deep link survives sign-in', () => {
    vi.mocked(useAuth).mockReturnValue({ user: null, isLoading: false } as ReturnType<typeof useAuth>);
    renderAt('/portal/firma?categoryId=abc-123&orderBy=Title');
    expect(screen.getByText('Giriş sayfası (from: /portal/firma?categoryId=abc-123&orderBy=Title)')).toBeInTheDocument();
  });

  it('redirects to the unauthorized page when the role does not match', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { userId: 'u-1', email: 'a@b.com', role: 'Candidate', status: 'Active', createdAtUtc: '', emailConfirmed: true },
      isLoading: false,
    } as ReturnType<typeof useAuth>);
    renderAt('/portal/firma');
    expect(screen.getByText('Yetkisiz sayfası')).toBeInTheDocument();
  });

  it('renders the protected content for a matching role', () => {
    vi.mocked(useAuth).mockReturnValue({
      user: { userId: 'u-1', email: 'a@b.com', role: 'Employer', status: 'Active', createdAtUtc: '', emailConfirmed: true },
      isLoading: false,
    } as ReturnType<typeof useAuth>);
    renderAt('/portal/firma');
    expect(screen.getByText('Korunan içerik')).toBeInTheDocument();
  });
});
