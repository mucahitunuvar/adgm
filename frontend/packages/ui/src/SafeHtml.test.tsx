import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { SafeHtml } from './SafeHtml';

describe('SafeHtml', () => {
  it('renders the given markup inside a container', () => {
    const { container } = render(<SafeHtml html="<p>Backend tarafından temizlenmiş <strong>içerik</strong></p>" />);
    expect(container.querySelector('strong')?.textContent).toBe('içerik');
  });
});
