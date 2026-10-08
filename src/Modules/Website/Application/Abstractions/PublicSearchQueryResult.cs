using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Görev 3: Page is already filtered by type/source; TypeCounts is computed from the same lang/source/
// query-token filters but WITHOUT the type filter, so a client can facet by type for the current query.
public sealed record PublicSearchQueryResult(
    PagedResult<PublicSearchResultCandidate> Page,
    IReadOnlyDictionary<string, int> TypeCounts);
