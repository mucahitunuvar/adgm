using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;

internal static class ScheduleContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/schedule",
                async (Guid id, ScheduleContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ScheduleContentItemCommand(id, request.RowVersion, request.PublishAtUtc, request.UnpublishAtUtc);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentPublish)
            .RequireRateLimiting("authenticated")
            .WithName("ScheduleContentItem")
            .WithTags("Website");
    }
}
