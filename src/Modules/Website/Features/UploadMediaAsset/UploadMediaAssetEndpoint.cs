using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;

internal static class UploadMediaAssetEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/media",
                async (
                    IFormFile file,
                    [FromForm] string? folder,
                    [FromForm] string? altText,
                    [FromForm] string? caption,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    await using var stream = file.OpenReadStream();
                    var command = new UploadMediaAssetCommand(stream, file.FileName, file.ContentType, folder, altText, caption);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/media", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .DisableAntiforgery()
            .WithName("UploadMediaAsset")
            .WithTags("Website");
    }
}
