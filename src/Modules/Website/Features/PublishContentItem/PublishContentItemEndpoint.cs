using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentItem;

internal static class PublishContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{id:guid}/publish",
                async (Guid id, PublishContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new PublishContentItemCommand(id, request.RowVersion, request.PublishAtUtc, request.UnpublishAtUtc);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentPublish)
            .RequireRateLimiting("authenticated")
            .WithName("PublishContentItem")
            .WithTags("Website");
    }
}
