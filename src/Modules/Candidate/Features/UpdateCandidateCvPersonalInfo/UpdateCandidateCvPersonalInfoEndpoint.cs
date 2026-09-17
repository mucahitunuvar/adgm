using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

internal static class UpdateCandidateCvPersonalInfoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/personal-info",
                async (
                    Guid candidateCvId,
                    UpdateCandidateCvPersonalInfoRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCandidateCvPersonalInfoCommand(
                        candidateCvId,
                        request.Title,
                        request.GenderId,
                        request.BirthDate,
                        request.DriversLicenseTypeId,
                        request.NationalityId,
                        request.NetSalaryExpectation,
                        request.MilitaryStatusId,
                        request.DisabilityInfo);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCandidateCvPersonalInfo")
            .WithTags("Candidate");
    }
}
