using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;

internal static class UpdateMediaAssetEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/media/{id:guid}",
                async (Guid id, UpdateMediaAssetRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateMediaAssetCommand(
                        id, request.Folder, request.Source, request.UsagePermissionNote,
                        request.ContainsPersonalData, request.Translations);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateMediaAsset")
            .WithTags("Website");
    }
}
