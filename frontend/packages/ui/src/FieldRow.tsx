import type { ReactNode } from 'react';

/** Birkaç alanı (ör. "ad" + "sıra") aynı satırda yan yana düzenler; dar ekranda alt alta döker. */
export function FieldRow({ children }: { children: ReactNode }) {
  return <div className="field-row">{children}</div>;
}
