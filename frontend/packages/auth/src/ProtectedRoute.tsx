import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthProvider';
import type { UserRole } from './roles';

/**
 * Yalnız UX'tir, güvenlik sınırı değildir (FRONTEND-GUVENLIK.md §7). Yetkisiz kullanıcı
 * URL'yi doğrudan yazsa da backend ilgili isteği 403 ile reddeder.
 */
export function ProtectedRoute({
  roles,
  signInTo = '/giris',
  unauthorizedTo = '/yetkisiz',
}: {
  roles: readonly UserRole[];
  signInTo?: string;
  unauthorizedTo?: string;
}) {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) return null;
  if (!user) return <Navigate to={signInTo} replace state={{ from: location.pathname }} />;
  if (!roles.includes(user.role)) return <Navigate to={unauthorizedTo} replace />;
  return <Outlet />;
}
