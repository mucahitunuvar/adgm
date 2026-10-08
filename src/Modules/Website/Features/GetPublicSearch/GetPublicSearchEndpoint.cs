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
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPublicSearchQuery(q, lang, type, source)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? GetPublicSearchQueryHandler.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-search")
            .WithName("GetPublicSearch")
            .WithTags("Website");
    }
}
