using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;

internal static class GetEventRegistrationByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/contents/{contentItemId:guid}/event/registrations/{id:guid}",
                async (
                    Guid contentItemId, Guid id, ISender sender, ILogger<GetEventRegistrationByIdQuery> logger,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetEventRegistrationByIdQuery(contentItemId, id), cancellationToken);

                    if (result.IsSuccess)
                    {
                        try
                        {
                            var logResult = await sender.Send(
                                new RecordPersonalDataAccessCommand(PersonalDataEntityType.EventRegistration, id, PersonalDataAccessAction.View, null),
                                cancellationToken);
                            if (logResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record personal data access for event registration '{Id}': {ErrorCode}", id, logResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(ex, "Unexpected error while recording personal data access for event registration '{Id}'.", id);
                        }
                    }

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("GetEventRegistrationById")
            .WithTags("Website");
    }
}
