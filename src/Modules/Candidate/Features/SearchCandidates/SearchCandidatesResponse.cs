namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;

public sealed record SearchCandidatesResponse(
    IReadOnlyList<CandidateListItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
