using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

internal static class GetPublicSiteSettingsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/website/settings",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicSiteSettingsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .WithName("GetPublicSiteSettings")
            .WithTags("Website");
    }
}
