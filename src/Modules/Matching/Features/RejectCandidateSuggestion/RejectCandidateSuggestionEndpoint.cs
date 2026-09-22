using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;

internal static class RejectCandidateSuggestionEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidate-suggestions/{candidateSuggestionId:guid}/reject",
                async (Guid candidateSuggestionId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RejectCandidateSuggestionCommand(candidateSuggestionId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("RejectCandidateSuggestion")
            .WithTags("Matching");
    }
}
