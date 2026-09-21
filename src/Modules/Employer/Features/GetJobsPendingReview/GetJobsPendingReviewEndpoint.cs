using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetJobsPendingReview;

internal static class GetJobsPendingReviewEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-advisor/jobs/pending-review",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetJobsPendingReviewQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<JobResponse>>(StatusCodes.Status200OK)
            .WithName("GetJobsPendingReview")
            .WithTags("Employer");
    }
}
