'use client';

import { useState } from 'react';
import { Share2, Check } from 'lucide-react';

/** Dış pakete gerek yok: Web Share API varsa onu kullanır, yoksa bağlantıyı kopyalar. */
export function ShareButtons({ title }: { title: string }) {
  const [copied, setCopied] = useState(false);

  async function handleShare() {
    if (typeof navigator !== 'undefined' && 'share' in navigator) {
      try {
        await navigator.share({ title, url: window.location.href });
        return;
      } catch {
        // Kullanıcı paylaşımı kapattı veya desteklenmiyor - kopyalamaya düş.
      }
    }
    try {
      await navigator.clipboard.writeText(window.location.href);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Panoya erişim yoksa sessizce yok say (UX, güvenlik etkisi yok).
    }
  }

  return (
    <button type="button" className="button button--quiet share-buttons" onClick={handleShare}>
      {copied ? <Check aria-hidden="true" /> : <Share2 aria-hidden="true" />}
      {copied ? 'Bağlantı kopyalandı' : 'Paylaş'}
    </button>
  );
}
