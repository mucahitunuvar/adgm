using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateEducation;

internal static class UpdateEducationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/content/educations/{educationId:guid}",
                async (
                    Guid candidateCvId,
                    Guid educationId,
                    UpdateEducationRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateEducationCommand(
                        candidateCvId,
                        educationId,
                        request.EducationLevelId,
                        request.StartDate,
                        request.CompletionStatus,
                        request.EndDate,
                        request.DiplomaGradingSystemId,
                        request.DiplomaGrade,
                        request.SchoolId,
                        request.SchoolNameFreeText,
                        request.ProvinceId,
                        request.Description);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateEducation")
            .WithTags("Candidate");
    }
}
