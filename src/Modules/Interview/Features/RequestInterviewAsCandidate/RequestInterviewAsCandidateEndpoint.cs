using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;

internal static class RequestInterviewAsCandidateEndpoint
{
    private const string CandidateRole = "Candidate";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/interviews",
                async (RequestInterviewAsCandidateRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new RequestInterviewAsCandidateCommand(request.CompanyId);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/candidates/interviews", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CandidateRole))
            .RequireRateLimiting("authenticated")
            .WithName("RequestInterviewAsCandidate")
            .WithTags("Interview");
    }
}
