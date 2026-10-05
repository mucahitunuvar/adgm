using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;

internal static class UpdatePopupTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/popups/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, UpdatePopupTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePopupTranslationCommand(
                        id, languageCode, request.RowVersion, request.Title, request.Body, request.ButtonLabel);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdatePopupTranslation")
            .WithTags("Website");
    }
}
