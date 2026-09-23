using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;

internal static class CompleteDevelopmentPlanEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/development-plans/{developmentPlanId:guid}/complete",
                async (Guid developmentPlanId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new CompleteDevelopmentPlanCommand(developmentPlanId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CompleteDevelopmentPlan")
            .WithTags("CareerDevelopment");
    }
}
