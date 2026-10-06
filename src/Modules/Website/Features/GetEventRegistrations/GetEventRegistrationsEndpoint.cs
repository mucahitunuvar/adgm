using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

internal static class GetEventRegistrationsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations",
                async (
                    Guid contentItemId,
                    string? status,
                    string? search,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    ILogger<GetEventRegistrationsQuery> logger,
                    CancellationToken cancellationToken) =>
                {
                    var parsedStatus = status is not null && Enum.TryParse<EventRegistrationStatus>(status, true, out var s) ? s : (EventRegistrationStatus?)null;
                    var query = new GetEventRegistrationsQuery(contentItemId, parsedStatus, search)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    // §1 "Liste kişisel veri içerir → erişim kaydı View" - one entry per call, covering
                    // both the personal-data rows and the counter summary embedded in the same response.
                    if (result.IsSuccess)
                    {
                        try
                        {
                            var logResult = await sender.Send(
                                new RecordPersonalDataAccessCommand(
                                    PersonalDataEntityType.EventRegistration, null, PersonalDataAccessAction.View,
                                    $"contentItemId={contentItemId};status={status}"),
                                cancellationToken);
                            if (logResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record personal data access for the event registration list of content item '{ContentItemId}': {ErrorCode}",
                                    contentItemId, logResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(
                                ex, "Unexpected error while recording personal data access for the event registration list of content item '{ContentItemId}'.",
                                contentItemId);
                        }
                    }

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("GetEventRegistrations")
            .WithTags("Website");
    }
}
