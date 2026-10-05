using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.ExportNewsletterSubscribers;

internal static class ExportNewsletterSubscribersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/newsletter-subscribers/export",
                async (
                    string? status, string? language, ISender sender, ILogger<ExportNewsletterSubscribersQuery> logger,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ExportNewsletterSubscribersQuery(status, language), cancellationToken);
                    if (result.IsFailure)
                    {
                        return result.ToProblem();
                    }

                    try
                    {
                        var detail = $"status={status ?? "*"};language={language ?? "*"}";
                        var logResult = await sender.Send(
                            new RecordPersonalDataAccessCommand(PersonalDataEntityType.NewsletterSubscriber, null, PersonalDataAccessAction.Export, detail),
                            cancellationToken);
                        if (logResult.IsFailure)
                        {
                            logger.LogWarning(
                                "Failed to record personal data access for the newsletter subscriber export: {ErrorCode}", logResult.Error.Code);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(ex, "Unexpected error while recording personal data access for the newsletter subscriber export.");
                    }

                    var csvBytes = NewsletterSubscriberCsvFormatter.Format(result.Value);
                    return Results.Bytes(csvBytes, "text/csv; charset=utf-8", "newsletter-subscribers.csv");
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("ExportNewsletterSubscribers")
            .WithTags("Website");
    }
}
