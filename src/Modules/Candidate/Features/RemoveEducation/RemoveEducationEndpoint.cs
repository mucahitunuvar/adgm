using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveEducation;

internal static class RemoveEducationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/candidates/{candidateCvId:guid}/content/educations/{educationId:guid}",
                async (Guid candidateCvId, Guid educationId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new RemoveEducationCommand(candidateCvId, educationId), cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("RemoveEducation")
            .WithTags("Candidate");
    }
}
