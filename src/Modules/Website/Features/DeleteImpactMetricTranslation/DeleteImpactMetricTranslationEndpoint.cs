using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteImpactMetricTranslation;

internal static class DeleteImpactMetricTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // rowVersion travels as a JSON body, not a query parameter - see
        // DeleteVideoTranslationEndpoint's remarks for why (byte[] query binding and inferred DELETE
        // bodies both fail at runtime/startup respectively).
        app.MapDelete(
                "/api/v1/admin/website/impact-metrics/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, [FromBody] DeleteImpactMetricTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteImpactMetricTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteImpactMetricTranslation")
            .WithTags("Website");
    }
}
