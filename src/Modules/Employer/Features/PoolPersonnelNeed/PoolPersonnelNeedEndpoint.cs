using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.PoolPersonnelNeed;

internal static class PoolPersonnelNeedEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/career-advisor/personnel-needs/{personnelNeedId:guid}/pool",
                async (Guid personnelNeedId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new PoolPersonnelNeedCommand(personnelNeedId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .WithName("PoolPersonnelNeed")
            .WithTags("Employer");
    }
}
