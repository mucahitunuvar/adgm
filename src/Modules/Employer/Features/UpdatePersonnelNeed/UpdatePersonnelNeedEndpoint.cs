using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.UpdatePersonnelNeed;

internal static class UpdatePersonnelNeedEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/employer/personnel-needs/{personnelNeedId:guid}",
                async (Guid personnelNeedId, UpdatePersonnelNeedRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePersonnelNeedCommand(
                        personnelNeedId,
                        request.EmploymentTypeId,
                        request.WorkLocationTypeId,
                        request.PositionId,
                        request.DepartmentId,
                        request.Quantity,
                        request.ProvinceId,
                        request.ExperienceLevelId,
                        request.DetailsText,
                        request.GenderPreferenceIds,
                        request.MilitaryStatusPreferenceIds,
                        request.EducationLevelPreferenceIds,
                        request.DrivingLicensePreferenceIds);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("UpdatePersonnelNeed")
            .WithTags("Employer");
    }
}
