using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.ResetPassword;

internal static class ResetPasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/reset-password", async (ResetPasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ResetPasswordCommand(request.Token, request.NewPassword);
                var result = await sender.Send(command, cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("ResetPassword")
            .WithTags("Auth");
    }
}
