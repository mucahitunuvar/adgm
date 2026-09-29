using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;

internal static class SetContentItemCategoriesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/categories",
                async (Guid id, SetContentItemCategoriesRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new SetContentItemCategoriesCommand(id, request.RowVersion, request.CategoryIds), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetContentItemCategories")
            .WithTags("Website");
    }
}
