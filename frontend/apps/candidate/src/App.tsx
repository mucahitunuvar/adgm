import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '@gm/auth';
import { PortalLayout } from './PortalLayout';
import { ConnectionTestPage } from './ConnectionTestPage';

const queryClient = new QueryClient();

export function App() {
  return (
    // Tarayıcıdan her zaman kendi origin'ine (`/api/...`) gidilir: geliştirmede Vite'ın
    // server.proxy'si, üretimde IIS aynı origin'e yönlendirir (FRONTEND-MIMARI.md §5, K-03).
    // Backend origin'ine (VITE_BACKEND_ORIGIN) doğrudan gitmek CORS'a çarpar - backend CORS
    // yapılandırmaz ("CORS kullanılmaz (aynı origin)").
    <AuthProvider baseUrl={window.location.origin}>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter basename="/portal/aday">
          <PortalLayout>
            <Routes>
              <Route path="/" element={<ConnectionTestPage />} />
            </Routes>
          </PortalLayout>
        </BrowserRouter>
      </QueryClientProvider>
    </AuthProvider>
  );
}
