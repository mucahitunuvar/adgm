using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

internal static class ExportEventRegistrationsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations/export",
                async (
                    Guid contentItemId, string? status, ISender sender, ILogger<ExportEventRegistrationsQuery> logger,
                    CancellationToken cancellationToken) =>
                {
                    var parsedStatus = status is not null && Enum.TryParse<EventRegistrationStatus>(status, true, out var s)
                        ? s
                        : (EventRegistrationStatus?)null;

                    var result = await sender.Send(new ExportEventRegistrationsQuery(contentItemId, parsedStatus), cancellationToken);
                    if (result.IsFailure)
                    {
                        return result.ToProblem();
                    }

                    // §1 "Erişim kaydı Export (Detail: etkinlik ve filtre)" - EntityId null (a bulk
                    // export, not one row), same shape as GetEventRegistrations' own View logging.
                    try
                    {
                        var logResult = await sender.Send(
                            new RecordPersonalDataAccessCommand(
                                PersonalDataEntityType.EventRegistration, null, PersonalDataAccessAction.Export,
                                $"contentItemId={contentItemId};status={status ?? "*"}"),
                            cancellationToken);
                        if (logResult.IsFailure)
                        {
                            logger.LogWarning(
                                "Failed to record personal data access for the event registration export of content item '{ContentItemId}': {ErrorCode}",
                                contentItemId, logResult.Error.Code);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(
                            ex,
                            "Unexpected error while recording personal data access for the event registration export of content item '{ContentItemId}'.",
                            contentItemId);
                    }

                    var csvBytes = EventRegistrationCsvFormatter.Format(result.Value);
                    return Results.Bytes(csvBytes, "text/csv; charset=utf-8", "event-registrations.csv");
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("ExportEventRegistrations")
            .WithTags("Website");
    }
}
