using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.GetNewsletterSubscribers;

internal static class GetNewsletterSubscribersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/admin/website/newsletter-subscribers",
                async (
                    string? status,
                    string? language,
                    string? search,
                    int? page,
                    int? pageSize,
                    ISender sender,
                    ILogger<GetNewsletterSubscribersQuery> logger,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetNewsletterSubscribersQuery(status, language, search)
                    {
                        Page = page ?? 1,
                        PageSize = pageSize ?? PagedRequest.DefaultPageSize,
                    };
                    var result = await sender.Send(query, cancellationToken);

                    // ADR-024 §14: unlike GetFormSubmissions' list, this one DOES carry personal data
                    // (email addresses), so it writes its own access log entry - the same best-effort,
                    // never-fails-the-response shape GetFormSubmissionByIdEndpoint uses for its detail view.
                    if (result.IsSuccess)
                    {
                        try
                        {
                            var logResult = await sender.Send(
                                new RecordPersonalDataAccessCommand(PersonalDataEntityType.NewsletterSubscriber, null, PersonalDataAccessAction.View, null),
                                cancellationToken);
                            if (logResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record personal data access for the newsletter subscriber list: {ErrorCode}", logResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(ex, "Unexpected error while recording personal data access for the newsletter subscriber list.");
                        }
                    }

                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(WebsitePolicies.SubmissionsView)
            .RequireRateLimiting("authenticated")
            .WithName("GetNewsletterSubscribers")
            .WithTags("Website");
    }
}
