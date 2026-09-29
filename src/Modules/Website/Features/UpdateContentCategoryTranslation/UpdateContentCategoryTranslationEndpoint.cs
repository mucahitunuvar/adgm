using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

internal static class UpdateContentCategoryTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}/translations/{languageCode}",
                async (
                    Guid typeId, Guid id, string languageCode, UpdateContentCategoryTranslationRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateContentCategoryTranslationCommand(
                        typeId, id, languageCode, request.RowVersion, request.Name, request.Slug, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateContentCategoryTranslation")
            .WithTags("Website");
    }
}
