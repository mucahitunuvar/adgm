using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.CompareContentItemRevisions;

internal static class CompareContentItemRevisionsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{id:guid}/revisions/compare",
                async (Guid id, int from, string? to, string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new CompareContentItemRevisionsQuery(id, from, to, lang), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("CompareContentItemRevisions")
            .WithTags("Website");
    }
}
