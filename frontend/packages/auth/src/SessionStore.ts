import type { GetCurrentUserResponse, LoginResponse } from '@gm/api-client';
import type { UserRole } from './roles';

/**
 * Backend `LoginResponse` (Login ve RefreshAccessToken aynı şekli döner). OpenAPI'den üretilir
 * (K-05); elle yazılmış tip tutulmaz (bkz. docs/prompts/FRONTEND-OPENAPI-PRODUCES-PROMPT.md Görev 1).
 */
export type Session = LoginResponse;

/**
 * Backend `GetCurrentUserResponse`. Tek bir `role` alanı vardır; ayrı bir "yetkiler" listesi
 * dönmez (bkz. FRONTEND-ACIK-MADDELER.md §A8). OpenAPI'de `role` düz `string`'dir; burada bilinen
 * rol kümesine (`UserRole`) daraltılır — `createAuthController`'daki `toUser()` bu daraltmayı
 * (`isUserRole`) yapar ve bilinmeyen bir rol gelirse hata fırlatır.
 */
export type SessionUser = Omit<GetCurrentUserResponse, 'role'> & { role: UserRole };

/**
 * K-07: Oturum yalnız bu arayüzün arkasında tutulur. Geçici model yalnız bellek
 * uygulamasını (`MemorySessionStore`) kullanır; kalıcı modele geçilirse yalnız
 * bu paketteki uygulama değişir, çağıran kod değişmez.
 */
export interface SessionStore {
  getSession(): Session | null;
  getAccessToken(): string | null;
  setSession(session: Session | null): void;
  subscribe(listener: () => void): () => void;
}
