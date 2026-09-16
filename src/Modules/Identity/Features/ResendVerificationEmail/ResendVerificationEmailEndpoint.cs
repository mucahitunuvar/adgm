using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;

internal static class ResendVerificationEmailEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/auth/resend-verification-email",
                async (ResendVerificationEmailRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ResendVerificationEmailCommand(request.Email);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("ResendVerificationEmail")
            .WithTags("Auth");
    }
}
