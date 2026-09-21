using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;

internal static class GetMyPersonnelNeedsEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/employer/personnel-needs",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetMyPersonnelNeedsQuery(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .Produces<IReadOnlyList<PersonnelNeedResponse>>(StatusCodes.Status200OK)
            .WithName("GetMyPersonnelNeeds")
            .WithTags("Employer");
    }
}
