using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// GetCookieConsentSummary's raw projection (ADR-024 §13 Faz 3 Görev 7): "yalnızca toplu sayılar" -
// grouping by category happens in the query handler, once the (small) matching set is in memory, since
// Categories is a primitive collection column no relational provider here can GROUP BY/UNNEST
// consistently across both SqlServer and Sqlite.
public sealed record CookieConsentSummaryItem(CookieConsentAction Action, IReadOnlyList<ThirdPartyScriptCategory> Categories);
