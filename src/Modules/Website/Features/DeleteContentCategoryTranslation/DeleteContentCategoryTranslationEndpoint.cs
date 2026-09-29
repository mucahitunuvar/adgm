using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategoryTranslation;

internal static class DeleteContentCategoryTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}/translations/{languageCode}",
                async (
                    Guid typeId, Guid id, string languageCode, [FromBody] DeleteContentCategoryTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteContentCategoryTranslationCommand(typeId, id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteContentCategoryTranslation")
            .WithTags("Website");
    }
}
