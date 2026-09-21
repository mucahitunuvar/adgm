using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.RejectJob;

internal static class RejectJobEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/jobs/{jobId:guid}/reject",
                async (Guid jobId, RejectJobRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RejectJobCommand(jobId, request.Reason), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("RejectJob")
            .WithTags("Employer");
    }
}
