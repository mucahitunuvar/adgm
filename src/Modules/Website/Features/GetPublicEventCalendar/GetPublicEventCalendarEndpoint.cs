using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicEventCalendar;

internal static class GetPublicEventCalendarEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/events/{contentItemId:guid}/calendar.ics",
                async (Guid contentItemId, string? lang, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetPublicEventCalendarQuery(contentItemId, lang), cancellationToken);
                    return result.IsFailure ? result.ToProblem() : Results.Text(result.Value, "text/calendar; charset=utf-8");
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("GetPublicEventCalendar")
            .WithTags("Website");
    }
}
