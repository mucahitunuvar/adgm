import { useEffect } from 'react';

/**
 * Sekme kapatma/yenileme için tarayıcı uyarısı (`beforeunload`). Uygulama içi gezinme
 * (react-router `BrowserRouter`, veri yönlendiricisi değil — `useBlocker` burada çalışmaz)
 * için ayrıca her formun "Vazgeç"/"Listeye dön" eylemi `confirmDiscardChanges` ile sarılmalıdır.
 */
export function UnsavedChangesGuard({ isDirty }: { isDirty: boolean }) {
  useEffect(() => {
    if (!isDirty) return undefined;
    function onBeforeUnload(event: BeforeUnloadEvent) {
      event.preventDefault();
      event.returnValue = '';
    }
    window.addEventListener('beforeunload', onBeforeUnload);
    return () => window.removeEventListener('beforeunload', onBeforeUnload);
  }, [isDirty]);

  return null;
}

/** Kirli bir formdan uygulama içi çıkmadan önce onay ister; temizse doğrudan `true` döner. */
export function confirmDiscardChanges(isDirty: boolean): boolean {
  return !isDirty || window.confirm('Kaydedilmemiş değişiklikleriniz var. Devam etmek istediğinize emin misiniz?');
}
