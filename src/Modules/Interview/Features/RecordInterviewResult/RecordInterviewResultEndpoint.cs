using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

internal static class RecordInterviewResultEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/interviews/{interviewId:guid}/result",
                async (Guid interviewId, RecordInterviewResultRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new RecordInterviewResultCommand(interviewId, request.Outcome, request.ResultNotes);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("RecordInterviewResult")
            .WithTags("Interview");
    }
}
