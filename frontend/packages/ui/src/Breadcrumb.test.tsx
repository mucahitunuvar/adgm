import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Breadcrumb } from './Breadcrumb';

describe('Breadcrumb', () => {
  it('renders earlier items as links and the last item as plain current-page text', () => {
    render(<Breadcrumb items={[{ title: 'Ana sayfa', path: '/' }, { title: 'Haberler', path: '/haberler' }, { title: 'Örnek haber', path: '/haberler/ornek' }]} />);

    expect(screen.getByRole('link', { name: 'Ana sayfa' })).toHaveAttribute('href', '/');
    expect(screen.getByRole('link', { name: 'Haberler' })).toHaveAttribute('href', '/haberler');
    const current = screen.getByText('Örnek haber');
    expect(current.tagName).toBe('SPAN');
    expect(current).toHaveAttribute('aria-current', 'page');
  });

  it('renders nothing for an empty list', () => {
    const { container } = render(<Breadcrumb items={[]} />);
    expect(container).toBeEmptyDOMElement();
  });

  it('does not link an item whose path is rejected by safeUrl (e.g. javascript:)', () => {
    render(<Breadcrumb items={[{ title: 'Zararlı', path: 'javascript:alert(1)' }, { title: 'Son', path: '/son' }]} />);
    expect(screen.queryByRole('link', { name: 'Zararlı' })).not.toBeInTheDocument();
    expect(screen.getByText('Zararlı').tagName).toBe('SPAN');
  });
});
