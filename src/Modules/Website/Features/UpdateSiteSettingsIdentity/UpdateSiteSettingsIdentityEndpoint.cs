using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

internal static class UpdateSiteSettingsIdentityEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/settings/identity",
                async (UpdateSiteSettingsIdentityRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteSettingsIdentityCommand(
                        request.RowVersion, request.LogoLightMediaAssetId, request.LogoDarkMediaAssetId,
                        request.FaviconMediaAssetId, request.DefaultOgImageMediaId, request.Translations);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteSettingsIdentity")
            .WithTags("Website");
    }
}
