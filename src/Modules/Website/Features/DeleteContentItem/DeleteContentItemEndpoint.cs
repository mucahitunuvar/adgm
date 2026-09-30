using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItem;

internal static class DeleteContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // [FromBody] is explicit - MapDelete does not infer a body parameter automatically.
        app.MapDelete(
                "/api/v1/admin/website/contents/{id:guid}",
                async (Guid id, [FromBody] DeleteContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeleteContentItemCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteContentItem")
            .WithTags("Website");
    }
}
