using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.AddEducation;

internal static class AddEducationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/candidates/{candidateCvId:guid}/content/educations",
                async (Guid candidateCvId, AddEducationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new AddEducationCommand(
                        candidateCvId,
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

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/candidates/{candidateCvId}/content", new { id = result.Value })
                        : result.ToProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("AddEducation")
            .WithTags("Candidate");
    }
}
