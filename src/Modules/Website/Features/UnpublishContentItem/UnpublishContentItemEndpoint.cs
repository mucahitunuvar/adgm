using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;

internal static class UnpublishContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{id:guid}/unpublish",
                async (Guid id, UnpublishContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UnpublishContentItemCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentPublish)
            .RequireRateLimiting("authenticated")
            .WithName("UnpublishContentItem")
            .WithTags("Website");
    }
}
