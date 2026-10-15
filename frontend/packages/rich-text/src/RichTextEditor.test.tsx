import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { RichTextEditor } from './RichTextEditor';

describe('RichTextEditor', () => {
  it('renders an accessible editable region and a labeled toolbar', async () => {
    render(
      <RichTextEditor value="<p>Merhaba</p>" onChange={() => {}} onRequestImage={() => Promise.resolve(null)} ariaLabel="İçerik gövdesi" />,
    );

    await waitFor(() => expect(screen.getByRole('textbox', { name: 'İçerik gövdesi' })).toBeInTheDocument());
    expect(screen.getByRole('toolbar', { name: 'Biçimlendirme araç çubuğu' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Kalın' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Resim ekle' })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /video|youtube|göm/i })).not.toBeInTheDocument();
  });

  it('disables the editable region when disabled is true', async () => {
    render(<RichTextEditor value="<p>Merhaba</p>" onChange={() => {}} onRequestImage={() => Promise.resolve(null)} ariaLabel="İçerik" disabled />);

    await waitFor(() => expect(screen.getByRole('textbox', { name: 'İçerik' })).toHaveAttribute('contenteditable', 'false'));
  });

  it('image button calls onRequestImage and ignores a null (cancelled) result', async () => {
    const onRequestImage = vi.fn().mockResolvedValue(null);
    const onChange = vi.fn();
    render(<RichTextEditor value="<p></p>" onChange={onChange} onRequestImage={onRequestImage} ariaLabel="İçerik" />);

    await waitFor(() => screen.getByRole('button', { name: 'Resim ekle' }));
    screen.getByRole('button', { name: 'Resim ekle' }).click();

    await waitFor(() => expect(onRequestImage).toHaveBeenCalledTimes(1));
    expect(onChange).not.toHaveBeenCalledWith(expect.stringContaining('<img'));
  });
});
