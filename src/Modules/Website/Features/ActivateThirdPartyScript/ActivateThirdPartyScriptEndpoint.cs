using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivateThirdPartyScript;

internal static class ActivateThirdPartyScriptEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/third-party-scripts/{id:guid}/activate",
                async (Guid id, ActivateThirdPartyScriptRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivateThirdPartyScriptCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivateThirdPartyScript")
            .WithTags("Website");
    }
}
