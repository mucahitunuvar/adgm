using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.PublishHomeLayout;

internal static class PublishHomeLayoutEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/layouts/home/publish",
                async (PublishHomeLayoutRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new PublishHomeLayoutCommand(request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("PublishHomeLayout")
            .WithTags("Website");
    }
}
