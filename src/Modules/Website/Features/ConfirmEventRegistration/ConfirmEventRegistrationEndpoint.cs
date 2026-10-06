using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmEventRegistration;

internal static class ConfirmEventRegistrationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations/{id:guid}/confirm",
                async (Guid contentItemId, Guid id, ConfirmEventRegistrationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ConfirmEventRegistrationCommand(contentItemId, id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("ConfirmEventRegistration")
            .WithTags("Website");
    }
}
