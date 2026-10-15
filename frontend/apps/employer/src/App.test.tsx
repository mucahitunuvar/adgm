import { render, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { App } from './App';

function problemResponse(status: number) {
  return new Response(JSON.stringify({ title: 'Auth.Unauthorized', status }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  });
}

describe('App', () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn().mockResolvedValue(problemResponse(401));
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('only ever calls fetch against the current page origin (no cross-origin CORS call to the backend)', async () => {
    // BrowserRouter'ın basename'i (/portal/firma) ile eşleşsin ki alt ağaç (AuthProvider dahil) render olsun.
    window.history.pushState({}, '', '/portal/firma/');
    render(<App />);

    await waitFor(() => expect(fetchMock).toHaveBeenCalled());

    for (const call of fetchMock.mock.calls) {
      const url = call[0] as string;
      expect(url.startsWith(window.location.origin)).toBe(true);
    }
  });
});
