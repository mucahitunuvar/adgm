using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReinstateJob;

internal static class AdminReinstateJobEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/jobs/{jobId:guid}/reinstate",
                async (Guid jobId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AdminReinstateJobCommand(jobId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("AdminReinstateJob")
            .WithTags("Employer");
    }
}
