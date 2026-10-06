using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;

internal static class CancelEventScheduleEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/cancel",
                async (Guid contentItemId, CancelEventScheduleRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new CancelEventScheduleCommand(contentItemId, request.RowVersion, request.Reason), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("CancelEventSchedule")
            .WithTags("Website");
    }
}
