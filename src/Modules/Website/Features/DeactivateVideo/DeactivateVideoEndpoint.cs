using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateVideo;

internal static class DeactivateVideoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/videos/{id:guid}/deactivate",
                async (Guid id, DeactivateVideoRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateVideoCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateVideo")
            .WithTags("Website");
    }
}
