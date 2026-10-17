using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

internal static class GetPublicContentsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/contents",
                async (
                    string type,
                    string? lang,
                    string? category,
                    string? tag,
                    string? search,
                    string? from,
                    string? to,
                    string? featured,
                    string? page,
                    string? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var fromResult = OptionalQueryParameter.Parse<DateTime>(from, nameof(from));
                    if (fromResult.IsFailure) return fromResult.ToProblem();
                    var toResult = OptionalQueryParameter.Parse<DateTime>(to, nameof(to));
                    if (toResult.IsFailure) return toResult.ToProblem();
                    var featuredResult = OptionalQueryParameter.Parse<bool>(featured, nameof(featured));
                    if (featuredResult.IsFailure) return featuredResult.ToProblem();
                    var pageResult = OptionalQueryParameter.Parse<int>(page, nameof(page));
                    if (pageResult.IsFailure) return pageResult.ToProblem();
                    var pageSizeResult = OptionalQueryParameter.Parse<int>(pageSize, nameof(pageSize));
                    if (pageSizeResult.IsFailure) return pageSizeResult.ToProblem();

                    var query = new GetPublicContentsQuery(
                        type, lang, category, tag, search, fromResult.Value, toResult.Value, featuredResult.Value)
                    {
                        Page = pageResult.Value ?? 1,
                        PageSize = pageSizeResult.Value ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .Produces<PublicContentListResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetPublicContents")
            .WithTags("Website");
    }
}
