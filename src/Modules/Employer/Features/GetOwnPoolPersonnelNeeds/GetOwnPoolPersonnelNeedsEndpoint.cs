using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetOwnPoolPersonnelNeeds;

internal static class GetOwnPoolPersonnelNeedsEndpoint
{
    private const string CareerAdvisorRole = "CareerAdvisor";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/career-advisor/personnel-needs/own-pool",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetOwnPoolPersonnelNeedsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(CareerAdvisorRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<PersonnelNeedResponse>>(StatusCodes.Status200OK)
            .WithName("GetOwnPoolPersonnelNeeds")
            .WithTags("Employer");
    }
}
