import { beforeEach, describe, expect, it } from 'vitest';
import { CookieConsentAction } from '@gm/api-client';
import { createAnonymousConsentId, hasCategoryConsent, readConsent, writeConsent } from './consentStore';

function clearCookies() {
  document.cookie.split(';').forEach((part) => {
    const name = part.split('=')[0]?.trim();
    if (name) document.cookie = `${name}=; max-age=0; path=/`;
  });
}

describe('consentStore', () => {
  beforeEach(() => clearCookies());

  it('returns null when no consent cookie is set', () => {
    expect(readConsent()).toBeNull();
  });

  it('round-trips a written consent state', () => {
    const state = {
      consentId: createAnonymousConsentId(),
      policyVersion: 3,
      categories: ['Necessary', 'Analytics'],
      action: CookieConsentAction.Custom,
    };
    writeConsent(state);
    expect(readConsent()).toEqual(state);
  });

  it('ignores a malformed cookie value', () => {
    document.cookie = 'gm_consent=not-json; path=/';
    expect(readConsent()).toBeNull();
  });

  it('never stores the consent choice in localStorage or sessionStorage', () => {
    writeConsent({ consentId: 'x', policyVersion: 1, categories: ['Necessary'], action: CookieConsentAction.AcceptAll });
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('Necessary is always considered granted even without a stored state', () => {
    expect(hasCategoryConsent(null, 'Necessary')).toBe(true);
    expect(hasCategoryConsent(null, 'Analytics')).toBe(false);
  });

  it('respects the stored categories for non-Necessary checks', () => {
    const state = { consentId: 'x', policyVersion: 1, categories: ['Necessary', 'Marketing'], action: CookieConsentAction.Custom };
    expect(hasCategoryConsent(state, 'Marketing')).toBe(true);
    expect(hasCategoryConsent(state, 'Analytics')).toBe(false);
  });
});
