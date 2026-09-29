using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;

internal static class SetContentItemRelatedContentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/related",
                async (Guid id, SetContentItemRelatedContentRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new SetContentItemRelatedContentCommand(id, request.RowVersion, request.RelatedContentItemIds), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetContentItemRelatedContent")
            .WithTags("Website");
    }
}
