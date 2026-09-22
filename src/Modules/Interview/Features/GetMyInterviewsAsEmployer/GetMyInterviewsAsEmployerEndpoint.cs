using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsEmployer;

internal static class GetMyInterviewsAsEmployerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/employer/interviews",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMyInterviewsAsEmployerQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<InterviewResponse>>(StatusCodes.Status200OK)
            .WithName("GetMyInterviewsAsEmployer")
            .WithTags("Interview");
    }
}
