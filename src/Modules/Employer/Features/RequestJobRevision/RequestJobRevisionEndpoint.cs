using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;

internal static class RequestJobRevisionEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/jobs/{jobId:guid}/request-revision",
                async (Guid jobId, RequestJobRevisionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RequestJobRevisionCommand(jobId, request.Notes), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("RequestJobRevision")
            .WithTags("Employer");
    }
}
