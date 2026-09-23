using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;

internal static class GetSiteLanguagesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/languages",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetSiteLanguagesQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetSiteLanguages")
            .WithTags("Website");
    }
}
