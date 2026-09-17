using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContentSummary;

internal static class UpdateCandidateCvContentSummaryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/summary",
                async (
                    Guid candidateCvId,
                    UpdateCandidateCvContentSummaryRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCandidateCvContentSummaryCommand(
                        candidateCvId, request.Summary, request.ComputerSkills, request.Hobbies);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCandidateCvContentSummary")
            .WithTags("Candidate");
    }
}
