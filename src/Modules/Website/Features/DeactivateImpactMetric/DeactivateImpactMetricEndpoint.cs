using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateImpactMetric;

internal static class DeactivateImpactMetricEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/impact-metrics/{id:guid}/deactivate",
                async (Guid id, DeactivateImpactMetricRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateImpactMetricCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateImpactMetric")
            .WithTags("Website");
    }
}
