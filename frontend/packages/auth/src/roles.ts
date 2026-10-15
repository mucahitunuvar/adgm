import { UserRole as UserRoleCodes } from '@gm/api-client';

/**
 * Backend rolleri. Kod listesinin TEK kaynağı `docs/api/enums.v1.json` (Identity.UserRole,
 * `pnpm api:gen` ile `@gm/api-client`'a üretilir); burada elle dizgi sabiti YAZILMAZ.
 */
export const USER_ROLES = Object.values(UserRoleCodes) as readonly UserRole[];

export type UserRole = UserRoleCodes;

export function isUserRole(value: string): value is UserRole {
  return (USER_ROLES as readonly string[]).includes(value);
}
