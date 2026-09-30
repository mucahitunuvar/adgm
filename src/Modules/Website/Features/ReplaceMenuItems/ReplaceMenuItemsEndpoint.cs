using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

internal static class ReplaceMenuItemsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/menus/{location}",
                async (string location, ReplaceMenuItemsRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new ReplaceMenuItemsCommand(location, request.RowVersion, request.Items), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ReplaceMenuItems")
            .WithTags("Website");
    }
}
