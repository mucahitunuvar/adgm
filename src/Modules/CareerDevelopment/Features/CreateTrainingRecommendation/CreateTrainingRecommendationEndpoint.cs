using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;

internal static class CreateTrainingRecommendationEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidates/{candidateCvId:guid}/training-recommendations",
                async (Guid candidateCvId, CreateTrainingRecommendationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateTrainingRecommendationCommand(
                        candidateCvId, request.DevelopmentPlanId, request.TrainingId, request.Notes);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/candidates/{candidateCvId}/training-recommendations", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateTrainingRecommendation")
            .WithTags("CareerDevelopment");
    }
}
