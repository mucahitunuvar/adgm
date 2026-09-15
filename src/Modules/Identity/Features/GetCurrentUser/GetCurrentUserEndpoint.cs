using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;

internal static class GetCurrentUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/me", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);

                return result.ToOkOrProblem();
            })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("GetCurrentUser")
            .WithTags("Auth");
    }
}
