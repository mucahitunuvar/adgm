namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;

public sealed record GetCandidateNotesResponse(
    IReadOnlyList<CandidateNoteItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
