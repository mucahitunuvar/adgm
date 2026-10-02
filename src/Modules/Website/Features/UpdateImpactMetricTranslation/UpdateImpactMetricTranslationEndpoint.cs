using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;

internal static class UpdateImpactMetricTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/impact-metrics/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdateImpactMetricTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateImpactMetricTranslationCommand(
                        id, languageCode, request.RowVersion, request.Label, request.Unit, request.Period, request.Source);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateImpactMetricTranslation")
            .WithTags("Website");
    }
}
