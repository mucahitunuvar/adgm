using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.ForgotPassword;

internal static class ForgotPasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/forgot-password", async (ForgotPasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ForgotPasswordCommand(request.Email);
                var result = await sender.Send(command, cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("ForgotPassword")
            .WithTags("Auth");
    }
}
