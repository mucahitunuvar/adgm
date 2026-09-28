using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateRedirect;

internal static class CreateRedirectEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/redirects",
                async (CreateRedirectRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateRedirectCommand(
                        request.LanguageCode, request.FromPath, request.TargetKind, request.TargetContentItemId,
                        request.TargetPath, request.StatusCode);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/redirects", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.StructureManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateRedirect")
            .WithTags("Website");
    }
}
