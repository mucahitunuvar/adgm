namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;

public sealed record GetEmploymentNotesResponse(
    IReadOnlyList<EmploymentNoteItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
