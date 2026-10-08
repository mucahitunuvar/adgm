using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;

internal static class RestoreContentItemRevisionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{id:guid}/revisions/{revisionNumber:int}/restore",
                async (
                    Guid id, int revisionNumber, RestoreContentItemRevisionRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RestoreContentItemRevisionCommand(
                        id, revisionNumber, request.RowVersion, request.LanguageCode, request.RestoreCategories);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("RestoreContentItemRevision")
            .WithTags("Website");
    }
}
