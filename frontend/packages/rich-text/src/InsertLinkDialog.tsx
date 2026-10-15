import { AccessibleDialog, Button, CheckboxField, TextField } from '@gm/ui';
import { useState } from 'react';
import { isAllowedLinkHref } from './isAllowedLinkHref';

export function InsertLinkDialog({
  open,
  initialHref,
  onCancel,
  onSubmit,
}: {
  open: boolean;
  initialHref: string;
  onCancel: () => void;
  onSubmit: (href: string, openInNewTab: boolean) => void;
}) {
  const [href, setHref] = useState(initialHref);
  const [openInNewTab, setOpenInNewTab] = useState(false);
  const isValid = isAllowedLinkHref(href);

  if (!open) return null;

  return (
    <AccessibleDialog open={open} onClose={onCancel} title="Bağlantı ekle">
      <form
        onSubmit={(event) => {
          event.preventDefault();
          if (isValid) onSubmit(href, openInNewTab);
        }}
      >
        <TextField
          id="rich-text-link-href"
          label="Adres"
          value={href}
          onChange={(event) => setHref(event.target.value)}
          hint="Site içi göreli bir yol (/ilanlar/ornek) veya http(s)/mailto/tel ile başlayan bir adres."
          error={href.length > 0 && !isValid ? 'Bu adres şekli desteklenmiyor.' : undefined}
        />
        <CheckboxField
          id="rich-text-link-new-tab"
          label="Yeni sekmede aç"
          checked={openInNewTab}
          onChange={(event) => setOpenInNewTab(event.target.checked)}
        />
        <div>
          <Button type="button" tone="quiet" onClick={onCancel}>
            Vazgeç
          </Button>{' '}
          <Button type="submit" disabled={!isValid}>
            Ekle
          </Button>
        </div>
      </form>
    </AccessibleDialog>
  );
}
