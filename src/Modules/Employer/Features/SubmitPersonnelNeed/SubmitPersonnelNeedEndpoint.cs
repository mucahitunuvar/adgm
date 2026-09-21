using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitPersonnelNeed;

internal static class SubmitPersonnelNeedEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/personnel-needs/{personnelNeedId:guid}/submit",
                async (Guid personnelNeedId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new SubmitPersonnelNeedCommand(personnelNeedId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("SubmitPersonnelNeed")
            .WithTags("Employer");
    }
}
