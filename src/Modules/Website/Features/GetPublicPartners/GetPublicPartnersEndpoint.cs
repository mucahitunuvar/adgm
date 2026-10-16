using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Application.Partners;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicPartners;

internal static class GetPublicPartnersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/partners",
                async (string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicPartnersQuery(lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<IReadOnlyList<PublicPartnerResponse>>(StatusCodes.Status200OK)
            .ProducesRateLimited()
            .WithName("GetPublicPartners")
            .WithTags("Website");
    }
}
