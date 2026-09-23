using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;

internal static class CreateCareerGoalEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidates/{candidateCvId:guid}/career-goals",
                async (Guid candidateCvId, CreateCareerGoalRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateCareerGoalCommand(candidateCvId, request.Description, request.TargetPositionId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/candidates/{candidateCvId}/career-goals", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateCareerGoal")
            .WithTags("CareerDevelopment");
    }
}
