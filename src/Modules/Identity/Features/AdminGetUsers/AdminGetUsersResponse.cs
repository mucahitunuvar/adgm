namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed record AdminGetUsersResponse(
    IReadOnlyList<AdminUserListItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
