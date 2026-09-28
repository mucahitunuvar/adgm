using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

internal static class UpdateContentTypeTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/content-types/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, UpdateContentTypeTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateContentTypeTranslationCommand(
                        id, languageCode, request.RowVersion, request.Name, request.RoutePrefix, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateContentTypeTranslation")
            .WithTags("Website");
    }
}
