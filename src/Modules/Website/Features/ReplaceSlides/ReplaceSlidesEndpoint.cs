using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

internal static class ReplaceSlidesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/sliders/{id:guid}/slides",
                async (Guid id, ReplaceSlidesRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ReplaceSlidesCommand(id, request.RowVersion, request.Slides), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ReplaceSlides")
            .WithTags("Website");
    }
}
