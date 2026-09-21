using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.UpdateJob;

internal static class UpdateJobEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/employer/jobs/{jobId:guid}",
                async (Guid jobId, UpdateJobRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateJobCommand(
                        jobId,
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

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("UpdateJob")
            .WithTags("Employer");
    }
}
