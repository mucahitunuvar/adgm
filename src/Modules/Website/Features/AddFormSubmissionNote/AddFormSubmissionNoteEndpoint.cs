using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.AddFormSubmissionNote;

internal static class AddFormSubmissionNoteEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/form-submissions/{id:guid}/notes",
                async (Guid id, AddFormSubmissionNoteRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AddFormSubmissionNoteCommand(id, request.RowVersion, request.Text), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("AddFormSubmissionNote")
            .WithTags("Website");
    }
}
