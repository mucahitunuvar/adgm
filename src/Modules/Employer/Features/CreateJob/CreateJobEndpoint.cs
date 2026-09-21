using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.CreateJob;

internal static class CreateJobEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/jobs",
                async (CreateJobRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateJobCommand(
                        request.Title,
                        request.IsForDisabledCandidates,
                        request.EmploymentTypeId,
                        request.WorkLocationTypeId,
                        request.PositionId,
                        request.DepartmentId,
                        request.ProvinceId,
                        request.DescriptionHtml,
                        request.ExperienceLevelId,
                        request.GenderPreferenceIds,
                        request.MilitaryStatusPreferenceIds,
                        request.EducationLevelPreferenceIds,
                        request.DrivingLicensePreferenceIds,
                        request.LanguageRequirements);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created("/api/v1/employer/jobs", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateJob")
            .WithTags("Employer");
    }
}
