using GenclikMerkezi.Modules.Interview.Features.CancelInterview;
using GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;
using GenclikMerkezi.Modules.Interview.Features.ScheduleInterview;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview;

public static class InterviewModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapInterviewModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RequestInterviewAsCandidateEndpoint.Map(app);
        RequestInterviewAsEmployerEndpoint.Map(app);
        ScheduleInterviewEndpoint.Map(app);
        CancelInterviewEndpoint.Map(app);
        RecordInterviewResultEndpoint.Map(app);

        return app;
    }
}
