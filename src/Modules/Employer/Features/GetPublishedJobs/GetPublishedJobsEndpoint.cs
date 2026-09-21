using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;

internal static class GetPublishedJobsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/jobs",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublishedJobsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<JobResponse>>(StatusCodes.Status200OK)
            .WithName("GetPublishedJobs")
            .WithTags("Employer");
    }
}
