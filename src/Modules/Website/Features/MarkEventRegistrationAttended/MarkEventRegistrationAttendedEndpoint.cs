using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationAttended;

internal static class MarkEventRegistrationAttendedEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations/{id:guid}/attended",
                async (Guid contentItemId, Guid id, MarkEventRegistrationAttendedRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new MarkEventRegistrationAttendedCommand(contentItemId, id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("MarkEventRegistrationAttended")
            .WithTags("Website");
    }
}
