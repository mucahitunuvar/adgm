using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;

internal static class ConvertNotFoundPathToRedirectEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/not-found-paths/{id:guid}/convert-to-redirect",
                async (Guid id, ConvertNotFoundPathToRedirectRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new ConvertNotFoundPathToRedirectCommand(
                        id, request.TargetKind, request.TargetContentItemId, request.TargetPath, request.StatusCode);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("ConvertNotFoundPathToRedirect")
            .WithTags("Website");
    }
}
