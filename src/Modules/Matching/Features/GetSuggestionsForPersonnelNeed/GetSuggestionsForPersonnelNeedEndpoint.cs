using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;

internal static class GetSuggestionsForPersonnelNeedEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-advisor/personnel-needs/{personnelNeedId:guid}/suggestions",
                async (Guid personnelNeedId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetSuggestionsForPersonnelNeedQuery(personnelNeedId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<CandidateSuggestionResponse>>(StatusCodes.Status200OK)
            .WithName("GetSuggestionsForPersonnelNeed")
            .WithTags("Matching");
    }
}
