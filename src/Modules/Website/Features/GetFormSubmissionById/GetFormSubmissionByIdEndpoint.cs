using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

internal static class GetFormSubmissionByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/form-submissions/{id:guid}",
                async (Guid id, ISender sender, ILogger<GetFormSubmissionByIdQuery> logger, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetFormSubmissionByIdQuery(id), cancellationToken);

                    // ADR-024 §12.2/§14: the read itself never writes (AGENTS.md §13) - the access log
                    // entry is a separate follow-up command, sent only once the read actually succeeded,
                    // and never allowed to turn into a 500 for the caller. Mirrors
                    // ResolveRouteEndpoint's NotFoundLog recording exactly.
                    if (result.IsSuccess)
                    {
                        try
                        {
                            var logResult = await sender.Send(
                                new RecordPersonalDataAccessCommand(PersonalDataEntityType.FormSubmission, id, PersonalDataAccessAction.View, null),
                                cancellationToken);
                            if (logResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record personal data access for form submission '{Id}': {ErrorCode}", id, logResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(ex, "Unexpected error while recording personal data access for form submission '{Id}'.", id);
                        }
                    }

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("GetFormSubmissionById")
            .WithTags("Website");
    }
}
