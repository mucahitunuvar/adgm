using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public sealed record AdminAuditLogFilter(
    Guid? TargetUserId,
    AdminActionType? ActionType,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize);
