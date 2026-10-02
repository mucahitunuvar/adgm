using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePartner;

internal static class DeactivatePartnerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/partners/{id:guid}/deactivate",
                async (Guid id, DeactivatePartnerRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DeactivatePartnerCommand(id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("DeactivatePartner")
            .WithTags("Website");
    }
}
