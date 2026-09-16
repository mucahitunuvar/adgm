using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

public sealed class AdminGetAuditLogQueryHandler(IAdminAuditLogRepository auditLogRepository)
    : IRequestHandler<AdminGetAuditLogQuery, Result<AdminGetAuditLogResponse>>
{
    public async Task<Result<AdminGetAuditLogResponse>> Handle(
        AdminGetAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        var actionType = request.ActionType is null
            ? (AdminActionType?)null
            : Enum.Parse<AdminActionType>(request.ActionType, ignoreCase: true);

        var filter = new AdminAuditLogFilter(
            request.TargetUserId,
            actionType,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize);

        var pagedEntries = await auditLogRepository.SearchAsync(filter, cancellationToken);

        var items = pagedEntries.Items
            .Select(e => new AdminAuditLogItemResponse(
                e.Id,
                e.AdminUserId,
                e.ActionType.ToString(),
                e.TargetUserId,
                e.OccurredAtUtc,
                e.Details,
                e.CorrelationId))
            .ToList();

        return Result.Success(new AdminGetAuditLogResponse(
            items,
            pagedEntries.TotalCount,
            pagedEntries.Page,
            pagedEntries.PageSize,
            pagedEntries.TotalPages));
    }
}
