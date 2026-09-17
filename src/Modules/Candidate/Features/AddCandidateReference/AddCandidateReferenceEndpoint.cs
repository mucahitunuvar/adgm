using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;

internal static class AddCandidateReferenceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/references",
                async (
                    Guid candidateCvId, AddCandidateReferenceRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddCandidateReferenceCommand(
                        candidateCvId,
                        request.ReferenceTypeId,
                        request.ReferenceLanguageId,
                        request.FirstName,
                        request.LastName,
                        request.Company,
                        request.Position,
                        request.Email,
                        request.PhoneNumber);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/candidates/{candidateCvId}/content", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("AddCandidateReference")
            .WithTags("Candidate");
    }
}
