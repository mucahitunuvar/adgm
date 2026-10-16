import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { JsonLd } from './JsonLd';

describe('JsonLd', () => {
  it('renders an application/ld+json script tag with the escaped payload', () => {
    const { container } = render(<JsonLd data={{ '@type': 'Organization', name: 'Test' }} />);
    const script = container.querySelector('script[type="application/ld+json"]');
    expect(script).not.toBeNull();
    expect(script?.textContent).toContain('"@type":"Organization"');
  });
});
