using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemapSegment;

internal static class GetSitemapSegmentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/sitemap-{segment:int}.xml",
                async (int segment, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetSitemapSegmentQuery(segment), cancellationToken);
                    return result.IsFailure ? result.ToProblem() : Results.Text(result.Value, "application/xml; charset=utf-8");
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetSitemapSegment")
            .WithTags("Website");
    }
}
