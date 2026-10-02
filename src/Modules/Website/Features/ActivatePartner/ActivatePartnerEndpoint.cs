using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePartner;

internal static class ActivatePartnerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/partners/{id:guid}/activate",
                async (Guid id, ActivatePartnerRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ActivatePartnerCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("ActivatePartner")
            .WithTags("Website");
    }
}
