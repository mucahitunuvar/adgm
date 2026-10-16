import { getPublicHome } from '@gm/api-client';
import { createServerApiClient } from '../lib/apiClient';

/**
 * Koşu 1 kararı: blok registry'si 15 tipin tamamını render eder (bu dosya Koşu 2C'de genişler).
 * Şimdilik (Koşu 2A) yalnız altyapının uçtan uca çalıştığını gösteren, gerçek veriyle çalışan
 * ama henüz tasarlanmamış bir çıktı - mock veri yok, blok listesi gerçek `GET /public/home`'dan.
 */
export async function HomePage() {
  const client = createServerApiClient();
  const home = await getPublicHome(client);

  if (home.blocks.length === 0) {
    return (
      <main className="container section">
        <p className="eyebrow">Ana sayfa</p>
        <p className="page-lead">Ana sayfa düzeni henüz yayınlanmamış.</p>
      </main>
    );
  }

  return (
    <main className="container section">
      <p className="eyebrow">Ana sayfa (Koşu 2C'de tasarlanacak)</p>
      <ul>
        {home.blocks.map((block, index) => (
          <li key={`${block.type}-${index}`}>{block.type}</li>
        ))}
      </ul>
    </main>
  );
}
