using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;

internal static class GetCandidateCvEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/candidates/{candidateCvId:guid}",
                async (Guid candidateCvId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCandidateCvQuery(candidateCvId), cancellationToken);

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<GetCandidateCvResponse>(StatusCodes.Status200OK)
            .WithName("GetCandidateCv")
            .WithTags("Candidate");
    }
}
