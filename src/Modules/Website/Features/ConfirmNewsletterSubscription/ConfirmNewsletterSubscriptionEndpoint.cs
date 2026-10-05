using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmNewsletterSubscription;

internal static class ConfirmNewsletterSubscriptionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/newsletter/confirmations",
                async (ConfirmNewsletterSubscriptionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ConfirmNewsletterSubscriptionCommand(request.Token), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("ConfirmNewsletterSubscription")
            .WithTags("Website");
    }
}
