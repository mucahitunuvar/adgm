using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

internal static class RegisterUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (RegisterUserRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(request.Email, request.Password, request.Role);
                var result = await sender.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/auth/users/{result.Value.UserId}", result.Value)
                    : result.ToProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("RegisterUser")
            .WithTags("Auth");
    }
}
