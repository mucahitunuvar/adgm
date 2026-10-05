using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.DownloadFormSubmissionFile;

internal static class DownloadFormSubmissionFileEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/form-submissions/{id:guid}/files/{fileId:guid}",
                async (
                    Guid id, Guid fileId, ISender sender, ILogger<DownloadFormSubmissionFileQuery> logger, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new DownloadFormSubmissionFileQuery(id, fileId), cancellationToken);
                    if (result.IsFailure)
                    {
                        return result.ToProblem();
                    }

                    try
                    {
                        var logResult = await sender.Send(
                            new RecordPersonalDataAccessCommand(PersonalDataEntityType.FormSubmission, id, PersonalDataAccessAction.DownloadFile, null),
                            cancellationToken);
                        if (logResult.IsFailure)
                        {
                            logger.LogWarning(
                                "Failed to record personal data access for form submission file '{Id}'/'{FileId}': {ErrorCode}",
                                id, fileId, logResult.Error.Code);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(ex, "Unexpected error while recording personal data access for form submission file '{Id}'/'{FileId}'.", id, fileId);
                    }

                    return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .Produces(StatusCodes.Status200OK)
            .WithName("DownloadFormSubmissionFile")
            .WithTags("Website");
    }
}
