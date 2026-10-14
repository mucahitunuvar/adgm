using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPartnerById;

internal static class GetPartnerByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/partners/{id:guid}",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPartnerByIdQuery(id), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.DesignManage)
            .RequireRateLimiting("authenticated")
            .Produces<PartnerDetailResponse>(StatusCodes.Status200OK)
            .ProducesUnauthenticated()
            .ProducesRoleForbidden()
            .ProducesNotFoundProblem()
            .ProducesRateLimited()
            .WithName("GetPartnerById")
            .WithTags("Website");
    }
}
