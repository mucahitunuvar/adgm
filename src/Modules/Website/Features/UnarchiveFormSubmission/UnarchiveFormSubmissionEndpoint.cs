using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveFormSubmission;

internal static class UnarchiveFormSubmissionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/form-submissions/{id:guid}/unarchive",
                async (Guid id, UnarchiveFormSubmissionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UnarchiveFormSubmissionCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("UnarchiveFormSubmission")
            .WithTags("Website");
    }
}
