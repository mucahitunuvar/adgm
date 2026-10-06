using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteEventSchedule;

internal static class DeleteEventScheduleEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event",
                async (Guid contentItemId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeleteEventScheduleCommand(contentItemId), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteEventSchedule")
            .WithTags("Website");
    }
}
