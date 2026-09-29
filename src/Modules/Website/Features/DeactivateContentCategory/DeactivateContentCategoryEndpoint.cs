using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentCategory;

internal static class DeactivateContentCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}/deactivate",
                async (Guid typeId, Guid id, DeactivateContentCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateContentCategoryCommand(typeId, id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeactivateContentCategory")
            .WithTags("Website");
    }
}
