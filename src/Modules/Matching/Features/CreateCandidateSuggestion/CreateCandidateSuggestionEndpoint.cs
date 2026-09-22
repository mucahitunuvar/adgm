using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;

internal static class CreateCandidateSuggestionEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidate-suggestions",
                async (CreateCandidateSuggestionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateCandidateSuggestionCommand(request.PersonnelNeedId, request.CandidateCvId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/career-advisor/candidate-suggestions", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateCandidateSuggestion")
            .WithTags("Matching");
    }
}
