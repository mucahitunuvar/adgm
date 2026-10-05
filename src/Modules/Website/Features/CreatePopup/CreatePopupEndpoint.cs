using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreatePopup;

internal static class CreatePopupEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/popups",
                async (CreatePopupRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreatePopupCommand(
                        request.DisplayMode, request.ImageMediaId, request.Link, request.Targeting, request.DeviceTarget,
                        request.PublishAtUtc, request.UnpublishAtUtc, request.DelaySeconds, request.Frequency, request.FrequencyDays,
                        request.Dismissible, request.Priority, request.DefaultLanguageTitle, request.DefaultLanguageBody,
                        request.DefaultLanguageButtonLabel);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/popups", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreatePopup")
            .WithTags("Website");
    }
}
