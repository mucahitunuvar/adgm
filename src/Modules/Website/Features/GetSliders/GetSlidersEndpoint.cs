using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetSliders;

internal static class GetSlidersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/sliders",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetSlidersQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<SliderSummaryResponse>>(StatusCodes.Status200OK)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesRateLimited()
            .WithName("GetSliders")
            .WithTags("Website");
    }
}
