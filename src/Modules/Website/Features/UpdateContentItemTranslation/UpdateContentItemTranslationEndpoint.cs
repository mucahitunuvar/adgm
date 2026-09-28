using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

internal static class UpdateContentItemTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, UpdateContentItemTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateContentItemTranslationCommand(
                        id, languageCode, request.RowVersion, request.Title, request.Slug, request.Summary, request.Body, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateContentItemTranslation")
            .WithTags("Website");
    }
}
