using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

internal static class ChangeFormSubmissionStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/form-submissions/{id:guid}/status",
                async (Guid id, ChangeFormSubmissionStatusRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new ChangeFormSubmissionStatusCommand(id, request.RowVersion, request.Status), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("ChangeFormSubmissionStatus")
            .WithTags("Website");
    }
}
