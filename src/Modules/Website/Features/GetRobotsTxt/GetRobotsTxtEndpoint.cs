using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetRobotsTxt;

internal static class GetRobotsTxtEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/robots.txt",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetRobotsTxtQuery(), cancellationToken);
                    return result.IsFailure ? result.ToProblem() : Results.Text(result.Value, "text/plain; charset=utf-8");
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetRobotsTxt")
            .WithTags("Website");
    }
}
