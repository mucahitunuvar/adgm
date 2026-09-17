namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;

public sealed record SearchCandidateCvsResponse(
    IReadOnlyList<CandidateCvListItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
