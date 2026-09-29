using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

internal static class CreateContentCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories",
                async (Guid typeId, CreateContentCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateContentCategoryCommand(
                        typeId, request.ParentId, request.SortOrder, request.DefaultLanguageName, request.DefaultLanguageSlug, request.Seo);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created($"/api/v1/admin/website/content-types/{typeId}/categories", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateContentCategory")
            .WithTags("Website");
    }
}
