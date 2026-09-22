using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.AdminSuspendJob;

internal static class AdminSuspendJobEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/jobs/{jobId:guid}/suspend",
                async (Guid jobId, AdminSuspendJobRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AdminSuspendJobCommand(jobId, request.Reason), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("AdminSuspendJob")
            .WithTags("Employer");
    }
}
