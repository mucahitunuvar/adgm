using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate;

public static class CandidateModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCandidateModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RegisterCandidateEndpoint.Map(app);

        return app;
    }
}
