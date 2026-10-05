using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;

internal static class AssignFormSubmissionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/form-submissions/{id:guid}/assign",
                async (Guid id, AssignFormSubmissionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new AssignFormSubmissionCommand(id, request.RowVersion, request.AssignedToUserId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("AssignFormSubmission")
            .WithTags("Website");
    }
}
