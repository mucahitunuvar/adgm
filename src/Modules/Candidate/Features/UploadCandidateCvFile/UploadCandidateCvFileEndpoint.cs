using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidateCvFile;

internal static class UploadCandidateCvFileEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/cv-file",
                async (Guid candidateCvId, IFormFile file, ISender sender, CancellationToken cancellationToken) =>
                {
                    await using var stream = file.OpenReadStream();
                    var command = new UploadCandidateCvFileCommand(candidateCvId, stream, file.FileName, file.ContentType);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .DisableAntiforgery()
            .Produces<string>(StatusCodes.Status200OK)
            .WithName("UploadCandidateCvFile")
            .WithTags("Candidate");
    }
}
