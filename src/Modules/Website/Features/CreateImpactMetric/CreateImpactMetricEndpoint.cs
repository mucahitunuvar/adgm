using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;

internal static class CreateImpactMetricEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/impact-metrics",
                async (CreateImpactMetricRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateImpactMetricCommand(
                        request.Value, request.IconKey, request.SortOrder, request.DefaultLanguageLabel, request.DefaultLanguageUnit,
                        request.DefaultLanguagePeriod, request.DefaultLanguageSource);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/impact-metrics", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateImpactMetric")
            .WithTags("Website");
    }
}
