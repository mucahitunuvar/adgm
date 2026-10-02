using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSlider;

internal static class UpdateSliderEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/sliders/{id:guid}",
                async (Guid id, UpdateSliderRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UpdateSliderCommand(id, request.RowVersion, request.Translations), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSlider")
            .WithTags("Website");
    }
}
