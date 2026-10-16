/** JS'siz GET formu: kategori seçimini korur, aramada sayfalama sıfırlanır. */
export function ContentSearchBox({ basePath, category, defaultValue }: { basePath: string; category?: string; defaultValue?: string }) {
  return (
    <form method="GET" action={basePath} className="content-search-box" role="search">
      {category && <input type="hidden" name="category" value={category} />}
      <label htmlFor="content-search-input" className="sr-only">
        İçerik ara
      </label>
      <input id="content-search-input" type="search" name="search" defaultValue={defaultValue} placeholder="Ara..." minLength={2} maxLength={100} />
      <button type="submit" className="button button--secondary">
        Ara
      </button>
    </form>
  );
}
