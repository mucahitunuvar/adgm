using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemap;

internal static class GetSitemapEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/sitemap.xml",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetSitemapQuery(), cancellationToken);
                    return result.IsFailure ? result.ToProblem() : Results.Text(result.Value, "application/xml; charset=utf-8");
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetSitemap")
            .WithTags("Website");
    }
}
