using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideo;

internal static class UpdateVideoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/videos/{id:guid}",
                async (Guid id, UpdateVideoRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateVideoCommand(
                        id, request.RowVersion, request.YouTubeUrl, request.CoverImageMediaId, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateVideo")
            .WithTags("Website");
    }
}
