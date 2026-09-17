using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCertificate;

internal static class RemoveCertificateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/candidates/{candidateCvId:guid}/content/certificates/{certificateId:guid}",
                async (Guid candidateCvId, Guid certificateId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RemoveCertificateCommand(candidateCvId, certificateId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RemoveCertificate")
            .WithTags("Candidate");
    }
}
