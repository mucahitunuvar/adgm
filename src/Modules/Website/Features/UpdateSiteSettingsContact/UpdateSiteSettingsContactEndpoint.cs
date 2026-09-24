using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;

internal static class UpdateSiteSettingsContactEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/settings/contact",
                async (UpdateSiteSettingsContactRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateSiteSettingsContactCommand(
                        request.RowVersion, request.Address, request.Phone, request.Email, request.WhatsApp,
                        request.MapEmbedUrl, request.SocialLinks);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateSiteSettingsContact")
            .WithTags("Website");
    }
}
