using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;

internal static class GetContentCategoriesByTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories",
                async (Guid typeId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetContentCategoriesByTypeQuery(typeId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetContentCategoriesByType")
            .WithTags("Website");
    }
}
