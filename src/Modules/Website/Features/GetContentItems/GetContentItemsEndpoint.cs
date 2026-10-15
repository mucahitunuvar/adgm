using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

internal static class GetContentItemsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents",
                async (
                    Guid? contentTypeId,
                    string? status,
                    string? languageCode,
                    string? search,
                    bool? isFeatured,
                    Guid? parentId,
                    Guid? categoryId,
                    Guid? tagId,
                    DateTime? publishedFromUtc,
                    DateTime? publishedToUtc,
                    DateTime? createdFromUtc,
                    DateTime? createdToUtc,
                    string? orderBy,
                    string? direction,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetContentItemsQuery(
                        contentTypeId, status, languageCode, search, isFeatured, parentId, categoryId, tagId, publishedFromUtc,
                        publishedToUtc, createdFromUtc, createdToUtc, orderBy, direction)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .Produces<PagedResult<ContentItemSummaryResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesRateLimited()
            .WithName("GetContentItems")
            .WithTags("Website");
    }
}
