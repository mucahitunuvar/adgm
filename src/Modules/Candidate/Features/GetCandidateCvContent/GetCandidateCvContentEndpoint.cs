using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

internal static class GetCandidateCvContentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates/{candidateCvId:guid}/content",
                async (Guid candidateCvId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCandidateCvContentQuery(candidateCvId), cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<GetCandidateCvContentResponse>(StatusCodes.Status200OK)
            .WithName("GetCandidateCvContent")
            .WithTags("Candidate");
    }
}
