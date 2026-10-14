using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

internal static class GetMenuByLocationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/menus/{location}",
                async (string location, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMenuByLocationQuery(location), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .Produces<MenuResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetMenuByLocation")
            .WithTags("Website");
    }
}
