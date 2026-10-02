using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentLayout;

internal static class GetContentLayoutEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/layouts/content/{contentItemId:guid}",
                async (Guid contentItemId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetContentLayoutQuery(contentItemId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetContentLayout")
            .WithTags("Website");
    }
}
