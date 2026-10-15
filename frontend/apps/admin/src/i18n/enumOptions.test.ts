import { UserStatus } from '@gm/api-client';
import { describe, expect, it } from 'vitest';
import { enumOptions } from './enumOptions';

describe('enumOptions', () => {
  it('sends the backend member name as value, and the Turkish translation as label', () => {
    const options = enumOptions(UserStatus, 'UserStatus');
    expect(options).toContainEqual({ value: 'Active', label: 'Aktif' });
    expect(options).toHaveLength(Object.keys(UserStatus).length);
  });

  it('excludes the given members', () => {
    const options = enumOptions(UserStatus, 'UserStatus', { exclude: [UserStatus.Deleted] });
    expect(options.map((o) => o.value)).not.toContain('Deleted');
  });

  it('only includes the given members, in that case', () => {
    const options = enumOptions(UserStatus, 'UserStatus', { only: [UserStatus.Active, UserStatus.Locked] });
    expect(options.map((o) => o.value).sort()).toEqual(['Active', 'Locked']);
  });
});
