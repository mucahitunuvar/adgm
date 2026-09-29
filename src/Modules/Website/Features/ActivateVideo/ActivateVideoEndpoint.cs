using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivateVideo;

internal static class ActivateVideoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/videos/{id:guid}/activate",
                async (Guid id, ActivateVideoRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivateVideoCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivateVideo")
            .WithTags("Website");
    }
}
