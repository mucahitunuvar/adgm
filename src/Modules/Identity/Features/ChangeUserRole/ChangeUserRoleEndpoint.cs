using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;

internal static class ChangeUserRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/auth/admin/users/{userId:guid}/role",
                async (Guid userId, ChangeUserRoleRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ChangeUserRoleCommand(userId, request.NewRole);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("authenticated")
            .WithName("ChangeUserRole")
            .WithTags("Auth");
    }
}
