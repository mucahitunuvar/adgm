using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidatePhoto;

internal static class UploadCandidatePhotoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/photo",
                async (Guid candidateCvId, IFormFile file, ISender sender, CancellationToken cancellationToken) =>
                {
                    await using var stream = file.OpenReadStream();
                    var command = new UploadCandidatePhotoCommand(candidateCvId, stream, file.FileName, file.ContentType);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .DisableAntiforgery()
            .Produces<string>(StatusCodes.Status200OK)
            .WithName("UploadCandidatePhoto")
            .WithTags("Candidate");
    }
}
