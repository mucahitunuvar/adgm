using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;

internal static class GetEventScheduleByContentItemIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event",
                async (Guid contentItemId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetEventScheduleByContentItemIdQuery(contentItemId), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetEventScheduleByContentItemId")
            .WithTags("Website");
    }
}
