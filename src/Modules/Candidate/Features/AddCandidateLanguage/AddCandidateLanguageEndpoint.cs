using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateLanguage;

internal static class AddCandidateLanguageEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/languages",
                async (
                    Guid candidateCvId, AddCandidateLanguageRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddCandidateLanguageCommand(
                        candidateCvId, request.LanguageId, request.LanguageLevelId, request.IsNativeLanguage);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/candidates/{candidateCvId}/content", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("AddCandidateLanguage")
            .WithTags("Candidate");
    }
}
