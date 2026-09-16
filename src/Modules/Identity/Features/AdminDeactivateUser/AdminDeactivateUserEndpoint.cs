using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminDeactivateUser;

internal static class AdminDeactivateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/auth/admin/users/{userId:guid}/deactivate",
                async (Guid userId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AdminDeactivateUserCommand(userId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("authenticated")
            .WithName("AdminDeactivateUser")
            .WithTags("Auth");
    }
}
