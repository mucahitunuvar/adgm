using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;

internal static class UpdateContentCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}",
                async (Guid typeId, Guid id, UpdateContentCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UpdateContentCategoryCommand(typeId, id, request.RowVersion, request.SortOrder), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesNotFoundProblem()
            .ProducesConflictProblem()
            .ProducesRateLimited()
            .WithName("UpdateContentCategory")
            .WithTags("Website");
    }
}
