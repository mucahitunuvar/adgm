using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;

internal static class CreateThirdPartyScriptEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/third-party-scripts",
                async (CreateThirdPartyScriptRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateThirdPartyScriptCommand(
                        request.Provider, request.MeasurementId, request.ContainerId, request.PixelId, request.Src, request.Async,
                        request.Defer, request.Category, request.Placement, request.SortOrder, request.DefaultLanguageName,
                        request.DefaultLanguagePurpose);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/third-party-scripts", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateThirdPartyScript")
            .WithTags("Website");
    }
}
