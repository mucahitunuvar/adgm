using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;

internal static class CreateSkillGapEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/candidates/{candidateCvId:guid}/skill-gaps",
                async (Guid candidateCvId, CreateSkillGapRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateSkillGapCommand(candidateCvId, request.SkillId, request.Notes);
                    var result = await sender.Send(command, cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"/api/v1/career-advisor/candidates/{candidateCvId}/skill-gaps", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("CreateSkillGap")
            .WithTags("CareerDevelopment");
    }
}
