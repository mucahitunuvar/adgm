using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;

internal static class UpdateSiteSettingsMaintenanceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/settings/maintenance",
                async (UpdateSiteSettingsMaintenanceRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteSettingsMaintenanceCommand(
                        request.RowVersion, request.MaintenanceModeEnabled, request.Translations);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteSettingsMaintenance")
            .WithTags("Website");
    }
}
