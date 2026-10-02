using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DiscardHomeLayoutDraft;

internal static class DiscardHomeLayoutDraftEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/layouts/home/discard-draft",
                async (DiscardHomeLayoutDraftRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DiscardHomeLayoutDraftCommand(request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DiscardHomeLayoutDraft")
            .WithTags("Website");
    }
}
