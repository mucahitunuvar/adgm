using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSearch;

public sealed record PublicSearchResponse(
    PagedResult<PublicSearchResultItemResponse> Results,
    IReadOnlyDictionary<string, int> TypeCounts);
