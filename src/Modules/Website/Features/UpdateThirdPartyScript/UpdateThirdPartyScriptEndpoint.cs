using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;

internal static class UpdateThirdPartyScriptEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/third-party-scripts/{id:guid}",
                async (Guid id, UpdateThirdPartyScriptRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateThirdPartyScriptCommand(
                        id, request.RowVersion, request.Provider, request.MeasurementId, request.ContainerId, request.PixelId, request.Src,
                        request.Async, request.Defer, request.Category, request.Placement, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateThirdPartyScript")
            .WithTags("Website");
    }
}
