using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

internal static class GetPublicSiteEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/site",
                async (string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicSiteQuery(lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .WithName("GetPublicSite")
            .WithTags("Website");
    }
}
