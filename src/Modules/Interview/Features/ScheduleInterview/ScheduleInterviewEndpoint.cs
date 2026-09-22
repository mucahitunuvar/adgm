using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.ScheduleInterview;

internal static class ScheduleInterviewEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/interviews/{interviewId:guid}/schedule",
                async (Guid interviewId, ScheduleInterviewRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ScheduleInterviewCommand(interviewId, request.ScheduledAtUtc);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("ScheduleInterview")
            .WithTags("Interview");
    }
}
