using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

internal static class GetCandidateDevelopmentSummaryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-development/candidates/{candidateCvId:guid}",
                async (Guid candidateCvId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCandidateDevelopmentSummaryQuery(candidateCvId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<GetCandidateDevelopmentSummaryResponse>(StatusCodes.Status200OK)
            .WithName("GetCandidateDevelopmentSummary")
            .WithTags("CareerDevelopment");
    }
}
