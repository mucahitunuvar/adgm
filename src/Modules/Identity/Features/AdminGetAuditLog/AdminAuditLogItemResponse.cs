namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed record AdminAuditLogItemResponse(
    Guid Id,
    Guid AdminUserId,
    string ActionType,
    Guid TargetUserId,
    DateTime OccurredAtUtc,
    string? Details,
    string? CorrelationId);
