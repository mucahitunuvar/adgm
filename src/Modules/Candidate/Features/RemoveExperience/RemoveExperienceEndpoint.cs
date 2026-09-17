using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveExperience;

internal static class RemoveExperienceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/candidates/{candidateCvId:guid}/content/experiences/{experienceId:guid}",
                async (Guid candidateCvId, Guid experienceId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RemoveExperienceCommand(candidateCvId, experienceId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RemoveExperience")
            .WithTags("Candidate");
    }
}
