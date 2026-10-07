using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;

internal static class GetPublicJobDetailEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/jobs/{slug}",
                async (string slug, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicJobDetailQuery(slug), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicJobDetail")
            .WithTags("Employer");
    }
}
