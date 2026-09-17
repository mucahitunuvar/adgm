using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed record AdminGetAuditLogQuery(
    Guid? TargetUserId,
    string? ActionType,
    DateTime? FromUtc,
    DateTime? ToUtc) : PagedRequest, IRequest<Result<AdminGetAuditLogResponse>>;
