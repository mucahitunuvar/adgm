using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

internal static class UpdateSiteSettingsBankAccountsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/settings/bank-accounts",
                async (UpdateSiteSettingsBankAccountsRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteSettingsBankAccountsCommand(request.RowVersion, request.BankAccounts);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteSettingsBankAccounts")
            .WithTags("Website");
    }
}
