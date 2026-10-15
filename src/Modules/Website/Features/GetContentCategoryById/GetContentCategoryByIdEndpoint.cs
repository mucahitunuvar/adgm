using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

internal static class GetContentCategoryByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/content-types/{typeId:guid}/categories/{id:guid}",
                async (Guid typeId, Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetContentCategoryByIdQuery(typeId, id), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .Produces<ContentCategoryDetailResponse>(StatusCodes.Status200OK)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetContentCategoryById")
            .WithTags("Website");
    }
}
