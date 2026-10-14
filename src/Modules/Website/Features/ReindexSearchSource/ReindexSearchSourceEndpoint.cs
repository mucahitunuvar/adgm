using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ReindexSearchSource;

internal static class ReindexSearchSourceEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/search/sources/{sourceKey}/reindex",
                async (string sourceKey, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ReindexSearchSourceCommand(sourceKey), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SettingsManage)
            .RequireRateLimiting("authenticated")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesNotFoundProblem()
            .ProducesConflictProblem()
            .ProducesRateLimited()
            .WithName("ReindexSearchSource")
            .WithTags("Website");
    }
}
