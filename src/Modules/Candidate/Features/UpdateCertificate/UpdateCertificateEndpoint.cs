using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;

internal static class UpdateCertificateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/certificates/{certificateId:guid}",
                async (
                    Guid candidateCvId,
                    Guid certificateId,
                    UpdateCertificateRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCertificateCommand(
                        candidateCvId, certificateId, request.Name, request.IssuingInstitution, request.CertificateDate, request.Description);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCertificate")
            .WithTags("Candidate");
    }
}
