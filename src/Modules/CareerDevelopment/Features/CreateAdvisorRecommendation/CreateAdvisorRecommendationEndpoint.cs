using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;

internal static class CreateAdvisorRecommendationEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidates/{candidateCvId:guid}/advisor-recommendations",
                async (Guid candidateCvId, CreateAdvisorRecommendationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateAdvisorRecommendationCommand(candidateCvId, request.Content);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/candidates/{candidateCvId}/advisor-recommendations", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateAdvisorRecommendation")
            .WithTags("CareerDevelopment");
    }
}
