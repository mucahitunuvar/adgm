using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview;

public static class InterviewModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapInterviewModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RequestInterviewAsCandidateEndpoint.Map(app);
        RequestInterviewAsEmployerEndpoint.Map(app);

        return app;
    }
}
