using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.RefreshAccessToken;

internal static class RefreshAccessTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/auth/refresh-token",
                async (RefreshAccessTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new RefreshAccessTokenCommand(request.RefreshToken);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("authenticated")
            .WithName("RefreshAccessToken")
            .WithTags("Auth");
    }
}
