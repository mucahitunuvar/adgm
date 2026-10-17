using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicVideos;

internal static class GetPublicVideosEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/videos",
                async (string? lang, string? page, string? pageSize, ISender sender, CancellationToken cancellationToken) =>
                {
                    var pageResult = OptionalQueryParameter.Parse<int>(page, nameof(page));
                    if (pageResult.IsFailure) return pageResult.ToProblem();
                    var pageSizeResult = OptionalQueryParameter.Parse<int>(pageSize, nameof(pageSize));
                    if (pageSizeResult.IsFailure) return pageSizeResult.ToProblem();

                    var query = new GetPublicVideosQuery(lang)
                    {
                        Page = pageResult.Value ?? 1,
                        PageSize = pageSizeResult.Value ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<PagedResult<PublicVideoResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesRateLimited()
            .WithName("GetPublicVideos")
            .WithTags("Website");
    }
}
