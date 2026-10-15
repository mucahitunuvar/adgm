import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { DateTimeField, istanbulLocalInputValueToUtcIso, utcIsoToIstanbulLocalInputValue } from './DateTimeField';

describe('utcIsoToIstanbulLocalInputValue / istanbulLocalInputValueToUtcIso', () => {
  it('converts UTC to Europe/Istanbul (+03:00, no DST) local input value', () => {
    expect(utcIsoToIstanbulLocalInputValue('2026-03-05T10:00:00Z')).toBe('2026-03-05T13:00');
  });

  it('round-trips back to the same UTC instant', () => {
    expect(istanbulLocalInputValueToUtcIso('2026-03-05T13:00')).toBe('2026-03-05T10:00:00.000Z');
  });

  it('handles a missing timezone suffix the same way formatDateTime.ts does', () => {
    expect(utcIsoToIstanbulLocalInputValue('2026-03-05T10:00:00')).toBe('2026-03-05T13:00');
  });

  it('returns an empty value for null/empty input', () => {
    expect(utcIsoToIstanbulLocalInputValue(null)).toBe('');
    expect(istanbulLocalInputValueToUtcIso('')).toBeNull();
  });
});

describe('DateTimeField', () => {
  it('shows the Istanbul-local value and reports the chosen instant back as UTC', () => {
    const onChange = vi.fn();
    render(<DateTimeField id="dt" label="Yayın tarihi" value="2026-03-05T10:00:00Z" onChange={onChange} />);

    const input = screen.getByLabelText(/Yayın tarihi/) as HTMLInputElement;
    expect(input.value).toBe('2026-03-05T13:00');

    fireEvent.change(input, { target: { value: '2026-04-01T09:30' } });

    expect(onChange).toHaveBeenLastCalledWith('2026-04-01T06:30:00.000Z');
  });
});
