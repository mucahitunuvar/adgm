using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;
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
                    if (result.IsSuccess && result.Value.Kind == nameof(RouteResolutionKind.NotFound))
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

                    return result.ToOkOrProblem();
                })
            .AllowAnonymous()
            .RequireRateLimiting("authenticated")
            .WithName("ResolveRoute")
            .WithTags("Website");
    }
}
