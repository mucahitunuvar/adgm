using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentType;

internal static class UpdateContentTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/content-types/{id:guid}",
                async (Guid id, UpdateContentTypeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateContentTypeCommand(
                        id, request.RowVersion, request.ListTemplate, request.DetailTemplate, request.SortMode, request.SortOrder,
                        request.SupportsHierarchy, request.SupportsCategories, request.SupportsTags, request.SupportsDetailImage,
                        request.SupportsGallery, request.SupportsVideos, request.SupportsAttachments, request.SupportsEvent,
                        request.SupportsBlockLayout, request.SupportsForm, request.SupportsRelatedContent, request.HasDetailPage,
                        request.HasListingPage, request.IsSearchable, request.RequiresReview);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateContentType")
            .WithTags("Website");
    }
}
