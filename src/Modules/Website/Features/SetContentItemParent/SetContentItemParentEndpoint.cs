using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemParent;

internal static class SetContentItemParentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{id:guid}/parent",
                async (Guid id, SetContentItemParentRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new SetContentItemParentCommand(id, request.RowVersion, request.ParentId);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("SetContentItemParent")
            .WithTags("Website");
    }
}
