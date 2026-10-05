using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ArchiveFormSubmission;

internal static class ArchiveFormSubmissionEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/form-submissions/{id:guid}/archive",
                async (Guid id, ArchiveFormSubmissionRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ArchiveFormSubmissionCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("ArchiveFormSubmission")
            .WithTags("Website");
    }
}
