using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentType;

internal static class DeactivateContentTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/content-types/{id:guid}/deactivate",
                async (Guid id, DeactivateContentTypeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivateContentTypeCommand(id, request.RowVersion), cancellationToken);
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
            .WithName("DeactivateContentType")
            .WithTags("Website");
    }
}
