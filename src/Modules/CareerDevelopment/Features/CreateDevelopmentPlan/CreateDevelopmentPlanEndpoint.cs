using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;

internal static class CreateDevelopmentPlanEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidates/{candidateCvId:guid}/development-plans",
                async (Guid candidateCvId, CreateDevelopmentPlanRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateDevelopmentPlanCommand(
                        candidateCvId, request.SkillGapId, request.CareerGoalId, request.Description);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/candidates/{candidateCvId}/development-plans", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateDevelopmentPlan")
            .WithTags("CareerDevelopment");
    }
}
