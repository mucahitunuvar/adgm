using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.Logout;

internal static class LogoutEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", async (LogoutRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new LogoutCommand(request.RefreshToken);
                var result = await sender.Send(command, cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .RequireAuthorization()
            .WithName("Logout")
            .WithTags("Auth");
    }
}
