using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;

internal static class CreatePersonnelNeedEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/personnel-needs",
                async (CreatePersonnelNeedRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreatePersonnelNeedCommand(
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

                    return result.IsSuccess
                        ? Results.Created("/api/v1/employer/personnel-needs", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreatePersonnelNeed")
            .WithTags("Employer");
    }
}
