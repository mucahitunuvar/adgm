using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CreateVideo;

internal static class CreateVideoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/videos",
                async (CreateVideoRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateVideoCommand(
                        request.YouTubeUrl, request.CoverImageMediaId, request.SortOrder,
                        request.DefaultLanguageTitle, request.DefaultLanguageDescription);
                    var result = await sender.Send(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.Created("/api/v1/admin/website/videos", result.Value)
                        : result.ToProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("CreateVideo")
            .WithTags("Website");
    }
}
