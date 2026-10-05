using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeletePopupTranslation;

internal static class DeletePopupTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/admin/website/popups/{id:guid}/translations/{languageCode}",
                async (
                    Guid id, string languageCode, [FromBody] DeletePopupTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeletePopupTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeletePopupTranslation")
            .WithTags("Website");
    }
}
