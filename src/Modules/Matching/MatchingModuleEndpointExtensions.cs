using GenclikMerkezi.Modules.Matching.Features.AcceptCandidateSuggestion;
using GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;
using GenclikMerkezi.Modules.Matching.Features.GetMySuggestions;
using GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;
using GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Matching;

public static class MatchingModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapMatchingModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateCandidateSuggestionEndpoint.Map(app);
        AcceptCandidateSuggestionEndpoint.Map(app);
        RejectCandidateSuggestionEndpoint.Map(app);
        GetSuggestionsForPersonnelNeedEndpoint.Map(app);
        GetMySuggestionsEndpoint.Map(app);

        return app;
    }
}
