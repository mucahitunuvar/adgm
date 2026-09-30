using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

internal static class GetContentPreviewEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/preview/{token}",
                async (string token, HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
                {
                    // ADR-024 §4.5 (Faz 1b Görev 6): a preview response is never cached and never
                    // indexed - it can show content nobody else can currently see.
                    httpContext.Response.Headers[HeaderNames.CacheControl] = "no-store";
                    httpContext.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";

                    var result = await sender.Send(new GetContentPreviewQuery(token), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetContentPreview")
            .WithTags("Website");
    }
}
