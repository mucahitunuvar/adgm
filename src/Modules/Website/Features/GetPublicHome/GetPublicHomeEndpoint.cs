using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicHome;

internal static class GetPublicHomeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/home",
                async (string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicHomeQuery(lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicHome")
            .WithTags("Website");
    }
}
