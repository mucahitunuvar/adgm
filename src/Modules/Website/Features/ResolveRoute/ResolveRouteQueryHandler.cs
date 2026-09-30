using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.RouteResolution;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

// ADR-024 §17 (Faz 1b Görev 7): NotFound outcomes are never cached (ADR-024 Faz 1b: caching every
// bot-guessed random path would otherwise grow the cache unbounded) - the 404 log and redirect-hit
// counters this endpoint sends afterwards are separate commands the endpoint always sends off the
// returned result, so they keep firing on every request regardless of whether this query itself was
// served from cache.
public sealed class ResolveRouteQueryHandler(RouteResolutionService routeResolutionService, ICacheService cacheService)
    : IRequestHandler<ResolveRouteQuery, Result<RouteResolutionResponse>>
{
    public async Task<Result<RouteResolutionResponse>> Handle(ResolveRouteQuery request, CancellationToken cancellationToken)
    {
        var normalizedPath = RoutePathFormat.Normalize(request.Path);
        var cacheKey = WebsiteCacheKeys.RouteResolution(normalizedPath);

        // MemoryCacheService.GetOrCreateAsync treats a cached null as a miss (re-invokes factory
        // instead of returning it), so returning null for NotFound here means a NotFound outcome is
        // effectively never served from cache - exactly the "NotFound sonuçları cache'lenmez" rule -
        // without ICacheService needing a separate "peek without writing" method just for this one
        // caller. resolvedNotFound captures that one resolution so a NotFound path is only resolved
        // once, not twice.
        RouteResolutionOutcome? resolvedNotFound = null;
        var cached = await cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var resolved = await routeResolutionService.ResolveAsync(request.Path, ct);
                if (resolved.Kind == RouteResolutionKind.NotFound)
                {
                    resolvedNotFound = resolved;
                    return null;
                }

                return resolved;
            },
            cancellationToken: cancellationToken);

        var outcome = cached ?? resolvedNotFound!;

        var alternates = outcome.Alternates?.Select(a => new RouteAlternateResponse(a.LanguageCode, a.Path)).ToList();
        var seo = outcome.Seo is null
            ? null
            : new RouteResolutionSeoResponse(
                outcome.Seo.MetaTitle, outcome.Seo.MetaDescription, outcome.Seo.MetaKeywords, outcome.Seo.OgTitle,
                outcome.Seo.OgDescription, outcome.Seo.OgImageMediaId, outcome.Seo.CanonicalUrl, outcome.Seo.NoIndex);

        var response = new RouteResolutionResponse(
            outcome.Kind.ToString(), outcome.LanguageCode, outcome.ContentItemId, outcome.ContentTypeKey, outcome.ListTemplate,
            outcome.DetailTemplate, outcome.Name, seo, alternates, outcome.Location, outcome.StatusCode, outcome.NotFoundLogPath,
            outcome.RedirectId);

        return Result.Success(response);
    }
}
