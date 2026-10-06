using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.AdminCancelEventRegistration;

internal static class AdminCancelEventRegistrationEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations/{id:guid}/cancel",
                async (Guid contentItemId, Guid id, AdminCancelEventRegistrationRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new AdminCancelEventRegistrationCommand(contentItemId, id, request.RowVersion), cancellationToken);
                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsManage)
            .RequireRateLimiting("authenticated")
            .WithName("AdminCancelEventRegistration")
            .WithTags("Website");
    }
}
