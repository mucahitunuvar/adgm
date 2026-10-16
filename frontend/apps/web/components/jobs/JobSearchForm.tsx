/**
 * JS'siz GET formu. `GetPublicJobs` doğrulanmış parametrelerin tamamı `q` ve
 * `isForDisabledCandidates` (bkz. BLOK-ESLEMESI.md önkoşul 3-a); il/pozisyon/departman gibi
 * kimlik (GUID) bazlı filtreler için public bir referans veri ucu yok (yalnız
 * `/admin/reference-data` yetkili), bu yüzden burada sunulmuyor - backend isteği olarak raporlandı.
 */
export function JobSearchForm({ defaultQuery, defaultDisabledOnly }: { defaultQuery?: string; defaultDisabledOnly?: boolean }) {
  return (
    <form method="GET" action="/ilanlar" className="job-search-form" role="search">
      <label htmlFor="job-search-input" className="sr-only">
        İlan ara
      </label>
      <input id="job-search-input" type="search" name="q" defaultValue={defaultQuery} placeholder="Pozisyon veya şirket adı ara..." minLength={2} maxLength={100} />
      <label className="checkbox-field checkbox-field--inline">
        <input type="checkbox" name="isForDisabledCandidates" value="true" defaultChecked={defaultDisabledOnly} />
        <span>Engelli kontenjanı</span>
      </label>
      <button type="submit" className="button button--primary">
        Ara
      </button>
    </form>
  );
}
