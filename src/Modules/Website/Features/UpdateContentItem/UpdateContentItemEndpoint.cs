using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItem;

internal static class UpdateContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}",
                async (Guid id, UpdateContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateContentItemCommand(
                        id, request.RowVersion, request.SortOrder, request.IsFeatured, request.CoverImageMediaId, request.DetailImageMediaId);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateContentItem")
            .WithTags("Website");
    }
}
