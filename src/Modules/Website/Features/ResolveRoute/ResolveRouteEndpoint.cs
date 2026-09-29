using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;
using GenclikMerkezi.Modules.Website.Features.RecordRedirectHit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

internal static class ResolveRouteEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/api/v1/public/routes/resolve",
                async (string? path, ISender sender, ILogger<ResolveRouteQuery> logger, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new ResolveRouteQuery(path), cancellationToken);

                    // ADR-024 §15: resolution itself is read-only (AGENTS.md §13: a query must not
                    // write) - the 404 log is written by this separate follow-up command, only when
                    // resolution actually landed on NotFound. A failure here must never affect the
                    // visitor's response, only be logged.
                    //
                    // Fix (post-Faz-1a review): logs NotFoundLogPath (the resolver's own normalized,
                    // language-prefix-stripped path), not the raw "path" query value - otherwise a
                    // 404 under a non-default language (e.g. "/en/xyz") is stored with the language
                    // prefix still attached, and a Redirect later created from it never matches what
                    // the resolver looks up ("xyz" under language "en") on the next request.
                    //
                    // Fix (post-Faz-1a review): the whole send is wrapped in try/catch (everything but
                    // cancellation) - a recording failure, however it happens, must never turn into a
                    // 500 for the visitor. RecordNotFoundPathCommandHandler already retries its own
                    // (LanguageCode, Path) race once and swallows the rest, so reaching this catch means
                    // something outside that handler's control went wrong (e.g. the database itself
                    // being unreachable) - still just logged, never surfaced.
                    if (result.IsSuccess && result.Value.Kind == nameof(RouteResolutionKind.NotFound))
                    {
                        try
                        {
                            var recordResult = await sender.Send(
                                new RecordNotFoundPathCommand(result.Value.LanguageCode, result.Value.NotFoundLogPath), cancellationToken);
                            if (recordResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record not-found path '{Path}' for language '{LanguageCode}': {ErrorCode}",
                                    path, result.Value.LanguageCode, recordResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(
                                ex, "Unexpected error while recording not-found path '{Path}' for language '{LanguageCode}'.",
                                path, result.Value.LanguageCode);
                        }
                    }

                    // Fix (post-Faz-1a review): Redirect.RecordHit existed but nothing ever called it,
                    // so HitCount stayed 0 forever. Mirrors the NotFound logging above exactly - a
                    // separate follow-up command (never the read-only resolution query itself), sent
                    // only when RedirectId is set (a real Redirect row, not a canonical-URL redirect
                    // synthesized from language-prefix-stripping or case/slash normalization), and
                    // wrapped the same way so a failure here can never affect the visitor's response.
                    if (result.IsSuccess && result.Value.Kind == nameof(RouteResolutionKind.Redirect) && result.Value.RedirectId is { } redirectId)
                    {
                        try
                        {
                            var recordResult = await sender.Send(new RecordRedirectHitCommand(redirectId), cancellationToken);
                            if (recordResult.IsFailure)
                            {
                                logger.LogWarning(
                                    "Failed to record a hit for redirect '{RedirectId}': {ErrorCode}", redirectId, recordResult.Error.Code);
                            }
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            logger.LogWarning(ex, "Unexpected error while recording a hit for redirect '{RedirectId}'.", redirectId);
                        }
                    }

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("public-read")
            .WithName("ResolveRoute")
            .WithTags("Website");
    }
}
