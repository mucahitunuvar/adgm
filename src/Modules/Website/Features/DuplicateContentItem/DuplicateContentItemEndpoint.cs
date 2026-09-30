using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DuplicateContentItem;

internal static class DuplicateContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{id:guid}/duplicate",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DuplicateContentItemCommand(id), cancellationToken);
                    return result.IsSuccess
                        ? Results.Created($"/api/v1/admin/website/contents/{result.Value.Id}", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DuplicateContentItem")
            .WithTags("Website");
    }
}
