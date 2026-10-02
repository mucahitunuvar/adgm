using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivateImpactMetric;

internal static class ActivateImpactMetricEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/impact-metrics/{id:guid}/activate",
                async (Guid id, ActivateImpactMetricRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivateImpactMetricCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivateImpactMetric")
            .WithTags("Website");
    }
}
