using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateLanguage;

internal static class UpdateCandidateLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/languages/{candidateLanguageId:guid}",
                async (
                    Guid candidateCvId,
                    Guid candidateLanguageId,
                    UpdateCandidateLanguageRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCandidateLanguageCommand(
                        candidateCvId, candidateLanguageId, request.LanguageId, request.LanguageLevelId, request.IsNativeLanguage);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCandidateLanguage")
            .WithTags("Candidate");
    }
}
