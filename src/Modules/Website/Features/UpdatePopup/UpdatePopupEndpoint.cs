using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopup;

internal static class UpdatePopupEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/popups/{id:guid}",
                async (Guid id, UpdatePopupRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePopupCommand(
                        id, request.RowVersion, request.DisplayMode, request.ImageMediaId, request.Link, request.Targeting,
                        request.DeviceTarget, request.PublishAtUtc, request.UnpublishAtUtc, request.DelaySeconds, request.Frequency,
                        request.FrequencyDays, request.Dismissible, request.Priority);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdatePopup")
            .WithTags("Website");
    }
}
