using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UnsubscribeFromNewsletter;

internal static class UnsubscribeFromNewsletterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/newsletter/unsubscriptions",
                async (UnsubscribeFromNewsletterRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UnsubscribeFromNewsletterCommand(request.Token), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("UnsubscribeFromNewsletter")
            .WithTags("Website");
    }
}
