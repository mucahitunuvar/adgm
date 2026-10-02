using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;

internal static class UpdateImpactMetricEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/impact-metrics/{id:guid}",
                async (Guid id, UpdateImpactMetricRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateImpactMetricCommand(id, request.RowVersion, request.Value, request.IconKey, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateImpactMetric")
            .WithTags("Website");
    }
}
