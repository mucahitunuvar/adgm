using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.CancelInterview;

internal static class CancelInterviewEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/interviews/{interviewId:guid}/cancel",
                async (Guid interviewId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new CancelInterviewCommand(interviewId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CancelInterview")
            .WithTags("Interview");
    }
}
