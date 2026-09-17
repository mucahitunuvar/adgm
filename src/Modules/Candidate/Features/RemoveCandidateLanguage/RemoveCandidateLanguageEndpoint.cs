using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateLanguage;

internal static class RemoveCandidateLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/candidates/{candidateCvId:guid}/content/languages/{candidateLanguageId:guid}",
                async (Guid candidateCvId, Guid candidateLanguageId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new RemoveCandidateLanguageCommand(candidateCvId, candidateLanguageId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RemoveCandidateLanguage")
            .WithTags("Candidate");
    }
}
