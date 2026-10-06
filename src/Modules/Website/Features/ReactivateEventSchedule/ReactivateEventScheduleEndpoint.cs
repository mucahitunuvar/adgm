using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReactivateEventSchedule;

internal static class ReactivateEventScheduleEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/reactivate",
                async (Guid contentItemId, ReactivateEventScheduleRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ReactivateEventScheduleCommand(contentItemId, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("ReactivateEventSchedule")
            .WithTags("Website");
    }
}
