using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSearch;

internal static class GetPublicSearchEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/search",
                async (
                    string q,
                    string? lang,
                    string? type,
                    string? source,
                    string? page,
                    string? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var pageResult = OptionalQueryParameter.Parse<int>(page, nameof(page));
                    if (pageResult.IsFailure) return pageResult.ToProblem();
                    var pageSizeResult = OptionalQueryParameter.Parse<int>(pageSize, nameof(pageSize));
                    if (pageSizeResult.IsFailure) return pageSizeResult.ToProblem();

                    var query = new GetPublicSearchQuery(q, lang, type, source)
                    {
                        Page = pageResult.Value ?? 1,
                        PageSize = pageSizeResult.Value ?? GetPublicSearchQueryHandler.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-search")
            .Produces<PublicSearchResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetPublicSearch")
            .WithTags("Website");
    }
}
