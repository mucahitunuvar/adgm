using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity.Features.VerifyEmail;

internal static class VerifyEmailEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // GET: for clicking the link directly from the verification email.
        app.MapGet("/api/v1/auth/verify-email", async (string token, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new VerifyEmailCommand(token), cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("VerifyEmailByLink")
            .WithTags("Auth");

        // POST: for a frontend that reads the token from the URL and confirms it via an API call,
        // keeping the token out of this endpoint's own server access logs.
        app.MapPost("/api/v1/auth/verify-email", async (VerifyEmailRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new VerifyEmailCommand(request.Token), cancellationToken);

                return result.ToNoContentOrProblem();
            })
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("VerifyEmail")
            .WithTags("Auth");
    }
}
