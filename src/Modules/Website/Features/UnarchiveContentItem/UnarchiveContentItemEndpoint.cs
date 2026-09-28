using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveContentItem;

internal static class UnarchiveContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{id:guid}/unarchive",
                async (Guid id, UnarchiveContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UnarchiveContentItemCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentPublish)
            .RequireRateLimiting("authenticated")
            .WithName("UnarchiveContentItem")
            .WithTags("Website");
    }
}
