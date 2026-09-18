using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Admin;

public static class AdminCareerAdvisorEndpoints
{
    private const string AdminRole = "Admin";

    public static IEndpointRouteBuilder MapAdminCareerAdvisorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/career-advisors/{careerAdvisorId:guid}/deactivate",
                async (Guid careerAdvisorId, CareerAdvisorDeactivationOrchestrator orchestrator, CancellationToken cancellationToken) =>
                {
                    var result = await orchestrator.DeactivateAndReassignAsync(careerAdvisorId, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateCareerAdvisorAndReassign")
            .WithTags("CareerAdvisor");

        return app;
    }
}
