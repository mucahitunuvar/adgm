using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentItem;

internal static class CreateContentItemEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents",
                async (CreateContentItemRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateContentItemCommand(
                        request.ContentTypeId, request.ParentId, request.SortOrder, request.IsFeatured, request.CoverImageMediaId,
                        request.DetailImageMediaId, request.DefaultLanguageTitle, request.DefaultLanguageSlug,
                        request.DefaultLanguageSummary, request.DefaultLanguageBody, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/contents", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateContentItem")
            .WithTags("Website");
    }
}
