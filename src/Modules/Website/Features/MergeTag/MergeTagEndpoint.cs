using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.MergeTag;

internal static class MergeTagEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/tags/{id:guid}/merge-into/{targetId:guid}",
                async (Guid id, Guid targetId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new MergeTagCommand(id, targetId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("MergeTag")
            .WithTags("Website");
    }
}
