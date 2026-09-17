using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.AddExperience;

internal static class AddExperienceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/experiences",
                async (Guid candidateCvId, AddExperienceRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddExperienceCommand(
                        candidateCvId,
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

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/candidates/{candidateCvId}/content", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("AddExperience")
            .WithTags("Candidate");
    }
}
