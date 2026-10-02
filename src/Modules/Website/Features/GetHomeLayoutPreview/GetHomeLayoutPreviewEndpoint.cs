using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayoutPreview;

internal static class GetHomeLayoutPreviewEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/layouts/home/preview",
                async (string? lang, HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
                {
                    httpContext.Response.Headers[HeaderNames.CacheControl] = "no-store";

                    var result = await sender.Send(new GetHomeLayoutPreviewQuery(lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetHomeLayoutPreview")
            .WithTags("Website");
    }
}
