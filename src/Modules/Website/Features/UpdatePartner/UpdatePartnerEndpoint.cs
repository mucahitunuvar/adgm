using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartner;

internal static class UpdatePartnerEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/partners/{id:guid}",
                async (Guid id, UpdatePartnerRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdatePartnerCommand(
                        id, request.RowVersion, request.LogoMediaId, request.WebsiteUrl, request.SortOrder);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpdatePartner")
            .WithTags("Website");
    }
}
