using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

internal static class UpsertEventScheduleEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event",
                async (Guid contentItemId, UpsertEventScheduleRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpsertEventScheduleCommand(
                        contentItemId, request.RowVersion, request.StartsAtUtc, request.EndsAtUtc, request.Format, request.OnlineLink,
                        request.Capacity, request.RegistrationEnabled, request.RegistrationOpensAtUtc, request.RegistrationClosesAtUtc,
                        request.MinAge, request.MaxAge, request.AutoConfirm, request.WaitlistEnabled, request.Translations);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.ContentManage)
            .RequireRateLimiting("authenticated")
            .WithName("UpsertEventSchedule")
            .WithTags("Website");
    }
}
