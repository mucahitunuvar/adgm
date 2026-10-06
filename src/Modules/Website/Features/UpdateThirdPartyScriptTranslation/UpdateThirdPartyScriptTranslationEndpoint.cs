using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;

internal static class UpdateThirdPartyScriptTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/third-party-scripts/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdateThirdPartyScriptTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateThirdPartyScriptTranslationCommand(id, languageCode, request.RowVersion, request.Name, request.Purpose);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateThirdPartyScriptTranslation")
            .WithTags("Website");
    }
}
