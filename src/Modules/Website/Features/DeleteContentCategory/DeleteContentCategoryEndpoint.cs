using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategory;

internal static class DeleteContentCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}",
                async (Guid typeId, Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeleteContentCategoryCommand(typeId, id), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeleteContentCategory")
            .WithTags("Website");
    }
}
