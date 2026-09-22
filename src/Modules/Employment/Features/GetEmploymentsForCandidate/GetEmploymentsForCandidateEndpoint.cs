using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;

internal static class GetEmploymentsForCandidateEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/employments/candidates/{candidateCvId:guid}",
                async (Guid candidateCvId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetEmploymentsForCandidateQuery(candidateCvId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<EmploymentResponse>>(StatusCodes.Status200OK)
            .WithName("GetEmploymentsForCandidate")
            .WithTags("Employment");
    }
}
