import { getApiErrorMessage, getCookieConsentSummary } from '@gm/api-client';
import { useAuth } from '@gm/auth';
import { Button, DateTimeField, EmptyState, ErrorState, LoadingState, PageHeader, ProgressBar } from '@gm/ui';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getEnumLabel } from '../../../i18n/getEnumLabel';

/** `byCategory`/`byAction` anahtarları `ThirdPartyScriptCategory`/`CookieConsentAction` üyeleridir (bkz. backend `GetCookieConsentSummaryQueryHandler`). */
function BreakdownBars({
  title,
  counts,
  total,
  enumName,
}: {
  title: string;
  counts: Record<string, number | string>;
  total: number;
  enumName: 'ThirdPartyScriptCategory' | 'CookieConsentAction';
}) {
  const entries = Object.entries(counts)
    .map(([key, value]) => [key, Number(value)] as const)
    .sort((a, b) => b[1] - a[1]);

  if (entries.length === 0) {
    return (
      <section>
        <h2>{title}</h2>
        <EmptyState title="Veri yok" description="Bu dönemde kayıt bulunmuyor." />
      </section>
    );
  }

  return (
    <section>
      <h2>{title}</h2>
      <table className="data-table">
        <caption className="sr-only">{title}</caption>
        <thead>
          <tr>
            <th scope="col">Kategori</th>
            <th scope="col" data-align="end">
              Sayı
            </th>
            <th scope="col">Dağılım</th>
          </tr>
        </thead>
        <tbody>
          {entries.map(([key, count]) => {
            const percent = total > 0 ? Math.round((count / total) * 1000) / 10 : 0;
            return (
              <tr key={key}>
                <td>{getEnumLabel(enumName, key)}</td>
                <td data-align="end">{count}</td>
                <td>
                  <ProgressBar value={percent} label={getEnumLabel(enumName, key)} />
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </section>
  );
}

export function CookieConsentSummaryPage() {
  const { client } = useAuth();
  const [draftRange, setDraftRange] = useState<{ from: string | null; to: string | null }>({ from: null, to: null });
  const [range, setRange] = useState<{ from: string | null; to: string | null }>({ from: null, to: null });

  const { data, isLoading, error } = useQuery({
    queryKey: ['website-cookie-consent-summary', range],
    queryFn: () => getCookieConsentSummary(client, { from: range.from ?? undefined, to: range.to ?? undefined }),
  });

  return (
    <div className="container section">
      <PageHeader
        eyebrow="Site yönetimi"
        title="Çerez özeti"
        description="Yalnız toplulaştırılmış sayılar; kişi bazlı veri yok (IP/User-Agent/kullanıcı kimliği hiç tutulmaz)."
      />
      <form
        className="filter-bar"
        onSubmit={(event) => {
          event.preventDefault();
          setRange(draftRange);
        }}
      >
        <div className="filter-bar__fields">
          <DateTimeField
            id="cookie-summary-from"
            label="Başlangıç"
            value={draftRange.from}
            onChange={(value) => setDraftRange((prev) => ({ ...prev, from: value }))}
          />
          <DateTimeField
            id="cookie-summary-to"
            label="Bitiş"
            value={draftRange.to}
            onChange={(value) => setDraftRange((prev) => ({ ...prev, to: value }))}
          />
        </div>
        <div className="filter-bar__actions">
          <Button type="submit" tone="secondary">
            Filtrele
          </Button>
          <Button
            type="button"
            tone="quiet"
            onClick={() => {
              setDraftRange({ from: null, to: null });
              setRange({ from: null, to: null });
            }}
          >
            Temizle
          </Button>
        </div>
      </form>

      {isLoading ? (
        <LoadingState />
      ) : error || !data ? (
        <ErrorState description={error ? getApiErrorMessage(error) : 'Özet yüklenemedi.'} />
      ) : (
        <div>
          <p>
            Toplam kayıt: <strong>{data.totalRecords}</strong>
          </p>
          <BreakdownBars title="Eyleme göre" counts={data.byAction} total={Number(data.totalRecords)} enumName="CookieConsentAction" />
          <BreakdownBars title="Kategoriye göre" counts={data.byCategory} total={Number(data.totalRecords)} enumName="ThirdPartyScriptCategory" />
        </div>
      )}
    </div>
  );
}
