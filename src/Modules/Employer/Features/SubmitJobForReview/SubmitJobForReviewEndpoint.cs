using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;

internal static class SubmitJobForReviewEndpoint
{
    private const string EmployerRole = "Employer";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/employer/jobs/{jobId:guid}/submit",
                async (Guid jobId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new SubmitJobForReviewCommand(jobId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(EmployerRole))
            .RequireRateLimiting("authenticated")
            .WithName("SubmitJobForReview")
            .WithTags("Employer");
    }
}
