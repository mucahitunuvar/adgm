using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisions;

internal static class GetContentItemRevisionsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{id:guid}/revisions",
                async (Guid id, int? page, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetContentItemRevisionsQuery(id)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("GetContentItemRevisions")
            .WithTags("Website");
    }
}
