using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyCompanyJobs;

internal static class GetMyCompanyJobsEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/employer/jobs",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMyCompanyJobsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<JobResponse>>(StatusCodes.Status200OK)
            .WithName("GetMyCompanyJobs")
            .WithTags("Employer");
    }
}
