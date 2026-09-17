using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateReference;

internal static class RemoveCandidateReferenceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/candidates/{candidateCvId:guid}/content/references/{candidateReferenceId:guid}",
                async (Guid candidateCvId, Guid candidateReferenceId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new RemoveCandidateReferenceCommand(candidateCvId, candidateReferenceId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RemoveCandidateReference")
            .WithTags("Candidate");
    }
}
