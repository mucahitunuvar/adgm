using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateRedirect;

internal static class UpdateRedirectEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/redirects/{id:guid}",
                async (Guid id, UpdateRedirectRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateRedirectCommand(id, request.TargetKind, request.TargetContentItemId, request.TargetPath, request.StatusCode);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateRedirect")
            .WithTags("Website");
    }
}
