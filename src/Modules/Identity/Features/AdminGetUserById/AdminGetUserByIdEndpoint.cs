using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;

internal static class AdminGetUserByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/auth/admin/users/{userId:guid}",
                async (Guid userId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AdminGetUserByIdQuery(userId), cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("authenticated")
            .WithName("AdminGetUserById")
            .WithTags("Auth");
    }
}
