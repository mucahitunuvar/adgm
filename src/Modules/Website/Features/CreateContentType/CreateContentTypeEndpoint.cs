using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentType;

internal static class CreateContentTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/content-types",
                async (CreateContentTypeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateContentTypeCommand(
                        request.Key, request.ListTemplate, request.DetailTemplate, request.SortMode, request.SortOrder,
                        request.SupportsHierarchy, request.SupportsCategories, request.SupportsTags, request.SupportsDetailImage,
                        request.SupportsGallery, request.SupportsVideos, request.SupportsAttachments, request.SupportsEvent,
                        request.SupportsBlockLayout, request.SupportsForm, request.SupportsRelatedContent, request.HasDetailPage,
                        request.HasListingPage, request.IsSearchable, request.RequiresReview,
                        request.DefaultLanguageName, request.DefaultLanguageRoutePrefix, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/content-types", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateContentType")
            .WithTags("Website");
    }
}
