using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;

internal static class ResendEventRegistrationVerificationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/event-registrations/verification-resends",
                async (
                    ResendEventRegistrationVerificationRequest request, HttpContext httpContext, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    var command = new ResendEventRegistrationVerificationCommand(
                        request.ContentItemId, request.SubmissionToken, request.TurnstileToken, request.Website, request.Email,
                        remoteIpAddress);

                    var result = await sender.Send(command, cancellationToken);
                    return result.ToAcceptedOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("ResendEventRegistrationVerification")
            .WithTags("Website");
    }
}
