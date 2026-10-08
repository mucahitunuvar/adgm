using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisionByNumber;

internal static class GetContentItemRevisionByNumberEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{id:guid}/revisions/{revisionNumber:int}",
                async (Guid id, int revisionNumber, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetContentItemRevisionByNumberQuery(id, revisionNumber), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetContentItemRevisionByNumber")
            .WithTags("Website");
    }
}
