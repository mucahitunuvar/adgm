using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateReference;

internal static class UpdateCandidateReferenceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/references/{candidateReferenceId:guid}",
                async (
                    Guid candidateCvId,
                    Guid candidateReferenceId,
                    UpdateCandidateReferenceRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCandidateReferenceCommand(
                        candidateCvId,
                        candidateReferenceId,
                        request.ReferenceTypeId,
                        request.ReferenceLanguageId,
                        request.FirstName,
                        request.LastName,
                        request.Company,
                        request.Position,
                        request.Email,
                        request.PhoneNumber);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCandidateReference")
            .WithTags("Candidate");
    }
}
