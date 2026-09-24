using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;

internal static class UpdateSiteSettingsFeaturesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/settings/features",
                async (UpdateSiteSettingsFeaturesRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteSettingsFeaturesCommand(
                        request.RowVersion, request.GlobalSearchEnabled, request.NewsletterEnabled,
                        request.PublicJobListingsEnabled, request.DonationPageEnabled);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteSettingsFeatures")
            .WithTags("Website");
    }
}
