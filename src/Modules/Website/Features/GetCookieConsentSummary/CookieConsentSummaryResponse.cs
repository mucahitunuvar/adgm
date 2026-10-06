namespace GenclikMerkezi.Modules.Website.Features.GetCookieConsentSummary;

// ADR-024 §13 (Faz 3 Görev 7): "yalnızca toplu sayılar (kategori ve eylem bazında); tekil kayıt listesi
// yok" - ByCategory counts how many recorded consents included each category (a record with several
// categories counts once per category, since Necessary alone would otherwise dwarf every other number
// without that context); ByAction counts records by their single Action.
public sealed record CookieConsentSummaryResponse(
    int TotalRecords, IReadOnlyDictionary<string, int> ByCategory, IReadOnlyDictionary<string, int> ByAction);
