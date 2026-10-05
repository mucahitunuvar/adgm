using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;

internal static class GetSubmissionTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/submission-tokens",
                async (HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
                {
                    // Each call issues a fresh, time-bound token - never something an intermediary cache
                    // should serve to more than one visitor.
                    httpContext.Response.Headers[HeaderNames.CacheControl] = "no-store";

                    var result = await sender.Send(new GetSubmissionTokenQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-forms")
            .WithName("GetSubmissionToken")
            .WithTags("Website");
    }
}
