using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DiscardContentLayoutDraft;

internal static class DiscardContentLayoutDraftEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/layouts/content/{contentItemId:guid}/discard-draft",
                async (Guid contentItemId, DiscardContentLayoutDraftRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DiscardContentLayoutDraftCommand(contentItemId, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DiscardContentLayoutDraft")
            .WithTags("Website");
    }
}
