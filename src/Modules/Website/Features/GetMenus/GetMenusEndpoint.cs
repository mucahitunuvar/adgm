using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetMenus;

internal static class GetMenusEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/menus",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMenusQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<MenuResponse>>(StatusCodes.Status200OK)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesRateLimited()
            .WithName("GetMenus")
            .WithTags("Website");
    }
}
