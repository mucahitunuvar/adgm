using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.VerifyEventRegistration;

internal static class VerifyEventRegistrationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/public/event-registrations/verifications",
                async (VerifyEventRegistrationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new VerifyEventRegistrationCommand(request.Token);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("VerifyEventRegistration")
            .WithTags("Website");
    }
}
