using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;

internal static class SetContentItemTranslationTagsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/translations/{languageCode}/tags",
                async (
                    Guid id, string languageCode, SetContentItemTranslationTagsRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new SetContentItemTranslationTagsCommand(id, languageCode, request.RowVersion, request.TagNames);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetContentItemTranslationTags")
            .WithTags("Website");
    }
}
