using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.Login;

internal static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new LoginCommand(request.Email, request.Password);
                var result = await sender.Send(command, cancellationToken);

                return result.ToOkOrProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("Login")
            .WithTags("Auth");
    }
}
