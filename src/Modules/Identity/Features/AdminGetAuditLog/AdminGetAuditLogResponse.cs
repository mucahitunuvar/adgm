namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed record AdminGetAuditLogResponse(
    IReadOnlyList<AdminAuditLogItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
