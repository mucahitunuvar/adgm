using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;

internal static class CreateCookieConsentRecordEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/cookie-consents",
                async (CreateCookieConsentRecordRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateCookieConsentRecordCommand(
                        request.ConsentId, request.Categories, request.PolicyVersion, request.Action);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToAcceptedOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("CreateCookieConsentRecord")
            .WithTags("Website");
    }
}
