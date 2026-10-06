using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventRegistration;

// §1 "guard gerektirmez, public-forms limiti uygulanır" - mirrors UnsubscribeFromNewsletterEndpoint's
// own no-guard, direct-token-lookup shape exactly.
internal static class CancelEventRegistrationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/event-registrations/cancellations",
                async (CancelEventRegistrationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CancelEventRegistrationCommand(request.Token);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("CancelEventRegistration")
            .WithTags("Website");
    }
}
