using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;

internal static class DeactivateCareerAdvisorEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/career-advisors/{careerAdvisorId:guid}/deactivate",
                async (Guid careerAdvisorId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateCareerAdvisorCommand(careerAdvisorId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateCareerAdvisor")
            .WithTags("CareerAdvisor");
    }
}
