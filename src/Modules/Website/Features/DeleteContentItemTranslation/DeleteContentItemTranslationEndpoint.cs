using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;

internal static class DeleteContentItemTranslationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // [FromBody] is explicit, not inferred: MapDelete does not infer a body parameter automatically
        // (see DeleteContentTypeTranslationEndpoint's remarks - verified against a real startup failure).
        app.MapDelete(
                "/api/v1/admin/website/contents/{id:guid}/translations/{languageCode}",
                async (Guid id, string languageCode, [FromBody] DeleteContentItemTranslationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteContentItemTranslationCommand(id, languageCode, request.RowVersion);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteContentItemTranslation")
            .WithTags("Website");
    }
}
