using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentLayout;

internal static class PublishContentLayoutEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/layouts/content/{contentItemId:guid}/publish",
                async (Guid contentItemId, PublishContentLayoutRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new PublishContentLayoutCommand(contentItemId, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("PublishContentLayout")
            .WithTags("Website");
    }
}
