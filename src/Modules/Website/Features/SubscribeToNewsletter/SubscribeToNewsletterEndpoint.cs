using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;

internal static class SubscribeToNewsletterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/newsletter/subscriptions",
                async (SubscribeToNewsletterRequest request, HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
                {
                    var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    var command = new SubscribeToNewsletterCommand(
                        request.SubmissionToken, request.TurnstileToken, request.Website, request.Email, request.Lang,
                        request.AcceptedPrivacyNoticeVersion, remoteIpAddress);

                    var result = await sender.Send(command, cancellationToken);
                    return result.ToAcceptedOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("SubscribeToNewsletter")
            .WithTags("Website");
    }
}
