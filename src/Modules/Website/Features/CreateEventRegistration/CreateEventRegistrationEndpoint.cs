using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;

internal static class CreateEventRegistrationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/events/{contentItemId:guid}/registrations",
                async (
                    Guid contentItemId, CreateEventRegistrationRequest request, HttpContext httpContext, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    var command = new CreateEventRegistrationCommand(
                        contentItemId, request.SubmissionToken, request.TurnstileToken, request.Website, request.FirstName,
                        request.LastName, request.Email, request.Phone, request.Lang, request.AcceptedPrivacyNoticeVersion,
                        remoteIpAddress);

                    var result = await sender.Send(command, cancellationToken);
                    return result.ToAcceptedOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("CreateEventRegistration")
            .WithTags("Website");
    }
}
