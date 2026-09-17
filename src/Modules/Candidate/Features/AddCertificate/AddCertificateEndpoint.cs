using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCertificate;

internal static class AddCertificateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/certificates",
                async (Guid candidateCvId, AddCertificateRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddCertificateCommand(
                        candidateCvId, request.Name, request.IssuingInstitution, request.CertificateDate, request.Description);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/candidates/{candidateCvId}/content", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("AddCertificate")
            .WithTags("Candidate");
    }
}
