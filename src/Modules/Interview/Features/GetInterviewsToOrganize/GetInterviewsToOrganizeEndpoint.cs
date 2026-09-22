using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.GetInterviewsToOrganize;

internal static class GetInterviewsToOrganizeEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-advisor/interviews/to-organize",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetInterviewsToOrganizeQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<InterviewResponse>>(StatusCodes.Status200OK)
            .WithName("GetInterviewsToOrganize")
            .WithTags("Interview");
    }
}
