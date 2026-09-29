using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdateTag;

internal static class UpdateTagEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/tags/{id:guid}",
                async (Guid id, UpdateTagRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new UpdateTagCommand(id, request.Name), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdateTag")
            .WithTags("Website");
    }
}
