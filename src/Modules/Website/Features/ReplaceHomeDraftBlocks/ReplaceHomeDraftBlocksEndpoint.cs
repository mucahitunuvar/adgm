using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;

internal static class ReplaceHomeDraftBlocksEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/layouts/home/draft",
                async (ReplaceHomeDraftBlocksRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ReplaceHomeDraftBlocksCommand(request.RowVersion, request.Blocks), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ReplaceHomeDraftBlocks")
            .WithTags("Website");
    }
}
