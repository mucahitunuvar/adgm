using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePopup;

internal static class ActivatePopupEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/popups/{id:guid}/activate",
                async (Guid id, ActivatePopupRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivatePopupCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivatePopup")
            .WithTags("Website");
    }
}
