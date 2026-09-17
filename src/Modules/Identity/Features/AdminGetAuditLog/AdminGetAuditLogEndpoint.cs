using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

internal static class AdminGetAuditLogEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/auth/admin/audit-log",
                async (
                    Guid? targetUserId,
                    string? actionType,
                    DateTime? fromUtc,
                    DateTime? toUtc,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new AdminGetAuditLogQuery(
                        targetUserId,
                        actionType,
                        fromUtc,
                        toUtc)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };

                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("authenticated")
            .Produces<AdminGetAuditLogResponse>(StatusCodes.Status200OK)
            .WithName("AdminGetAuditLog")
            .WithTags("Auth");
    }
}
