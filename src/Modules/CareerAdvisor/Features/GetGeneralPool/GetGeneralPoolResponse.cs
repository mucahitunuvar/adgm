namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;

public sealed record GetGeneralPoolResponse(
    IReadOnlyList<PersonnelNeedPoolItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
