using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

internal static class AdminGetUsersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/auth/admin/users",
                async (
                    string? email,
                    string? role,
                    string? status,
                    bool? isLockedOut,
                    bool? emailConfirmed,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new AdminGetUsersQuery(
                        email,
                        role,
                        status,
                        isLockedOut,
                        emailConfirmed,
                        page ?? 1,
                        pageSize ?? 20);

                    var result = await sender.Send(query, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("authenticated")
            .WithName("AdminGetUsers")
            .WithTags("Auth");
    }
}
