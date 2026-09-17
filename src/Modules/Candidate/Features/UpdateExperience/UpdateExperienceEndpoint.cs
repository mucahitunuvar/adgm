using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;

internal static class UpdateExperienceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/experiences/{experienceId:guid}",
                async (
                    Guid candidateCvId,
                    Guid experienceId,
                    UpdateExperienceRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateExperienceCommand(
                        candidateCvId,
                        experienceId,
                        request.CompanyName,
                        request.PositionId,
                        request.StartDate,
                        request.EndDate,
                        request.IsCurrentJob,
                        request.SectorId,
                        request.WorkFieldId,
                        request.EmploymentTypeId,
                        request.CountryId,
                        request.ProvinceId,
                        request.JobDescription);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateExperience")
            .WithTags("Candidate");
    }
}
