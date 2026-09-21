using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveJob;

internal static class ApproveJobEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/jobs/{jobId:guid}/approve",
                async (Guid jobId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ApproveJobCommand(jobId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("ApproveJob")
            .WithTags("Employer");
    }
}
