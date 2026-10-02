using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;

internal static class ReplaceContentDraftBlocksEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/layouts/content/{contentItemId:guid}/draft",
                async (Guid contentItemId, ReplaceContentDraftBlocksRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new ReplaceContentDraftBlocksCommand(contentItemId, request.RowVersion, request.Blocks), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ReplaceContentDraftBlocks")
            .WithTags("Website");
    }
}
