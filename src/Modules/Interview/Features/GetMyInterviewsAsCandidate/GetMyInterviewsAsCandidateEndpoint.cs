using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;

internal static class GetMyInterviewsAsCandidateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates/interviews",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMyInterviewsAsCandidateQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<InterviewResponse>>(StatusCodes.Status200OK)
            .WithName("GetMyInterviewsAsCandidate")
            .WithTags("Interview");
    }
}
