using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.RouteResolution;

// ADR-024 §15 (Faz 1a Görev 6): public route resolution spans SiteLanguage, ContentType, ContentItem
// and Redirect - a focused Application-layer service (the ContentPathCascadeService/RouteResolution
// split mirrors Görev 4's Domain/Application split: RoutePathFormat is pure path-string math, this
// class is the repository orchestration that math needs), not a generic "manager". The public
// ResolveRouteQueryHandler is this class's only caller.
public sealed class RouteResolutionService(
    ISiteLanguageRepository siteLanguageRepository,
    IContentTypeRepository contentTypeRepository,
    IContentItemRepository contentItemRepository,
    IRedirectRepository redirectRepository,
    ContentPathCascadeService contentPathCascadeService)
{
    public async Task<RouteResolutionOutcome> ResolveAsync(string? rawPath, CancellationToken cancellationToken)
    {
        var languages = await siteLanguageRepository.GetAllAsync(cancellationToken);
        var defaultLanguage = languages.First(l => l.IsDefault);

        var normalizedPath = RoutePathFormat.Normalize(rawPath);
        var (firstSegment, remainder) = RoutePathFormat.SplitFirstSegment(normalizedPath);
        var matchedLanguage = languages.FirstOrDefault(l => l.Code.Value == firstSegment);

        string languageCode;
        string pathAfterLanguage;
        if (matchedLanguage is not null)
        {
            // A passive language's code as a prefix never resolves to anything - reported under the
            // default language, since the request never actually reaches that (inactive) language.
            if (!matchedLanguage.IsActive)
            {
                return RouteResolutionOutcome.NotFound(defaultLanguage.Code.Value, normalizedPath);
            }

            if (matchedLanguage.IsDefault)
            {
                var canonicalWithoutPrefix = RoutePathFormat.BuildPublicPath(defaultLanguage.Code.Value, defaultLanguage.Code.Value, remainder);
                return RouteResolutionOutcome.Redirect(canonicalWithoutPrefix, (int)RedirectStatusCode.MovedPermanently);
            }

            languageCode = matchedLanguage.Code.Value;
            pathAfterLanguage = remainder;
        }
        else
        {
            languageCode = defaultLanguage.Code.Value;
            pathAfterLanguage = normalizedPath;
        }

        // One canonical-URL check covers both the default-language-prefix rule and the case/slash
        // normalization rule at once (§15: "tek kanonik URL") - pathAfterLanguage is already fully
        // normalized, so any remaining difference from what was actually requested is exactly the set
        // of fixes those two rules describe.
        var canonicalPublicPath = RoutePathFormat.BuildPublicPath(languageCode, defaultLanguage.Code.Value, pathAfterLanguage);
        var requestedPath = string.IsNullOrEmpty(rawPath) ? "/" : rawPath;
        if (!string.Equals(canonicalPublicPath, requestedPath, StringComparison.Ordinal))
        {
            return RouteResolutionOutcome.Redirect(canonicalPublicPath, (int)RedirectStatusCode.MovedPermanently);
        }

        if (pathAfterLanguage.Length == 0)
        {
            return RouteResolutionOutcome.Home(languageCode);
        }

        var languageCodeVo = LanguageCode.Create(languageCode).Value;
        var now = DateTime.UtcNow;

        var detailOutcome = await TryResolveDetailAsync(languageCode, languageCodeVo, pathAfterLanguage, defaultLanguage.Code.Value, languages, now, cancellationToken);
        if (detailOutcome is not null)
        {
            return detailOutcome;
        }

        if (!pathAfterLanguage.Contains('/'))
        {
            var listingOutcome = await TryResolveListingAsync(languageCode, languageCodeVo, pathAfterLanguage, defaultLanguage.Code.Value, languages);
            if (listingOutcome is not null)
            {
                return listingOutcome;
            }
        }

        var redirectOutcome = await TryResolveRedirectAsync(languageCode, languageCodeVo, pathAfterLanguage, defaultLanguage.Code.Value, now, cancellationToken);
        if (redirectOutcome is not null)
        {
            return redirectOutcome;
        }

        return RouteResolutionOutcome.NotFound(languageCode, pathAfterLanguage);
    }

    private async Task<RouteResolutionOutcome?> TryResolveDetailAsync(
        string languageCode, LanguageCode languageCodeVo, string pathAfterLanguage, string defaultLanguageCode,
        IReadOnlyList<SiteLanguage> languages, DateTime now, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByFullPathAsync(languageCodeVo, pathAfterLanguage, cancellationToken);
        if (contentItem is null)
        {
            return null;
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.IsActive || !contentType.HasDetailPage)
        {
            return null;
        }

        if (!await contentPathCascadeService.IsVisibleWithAncestorsAsync(contentItem, now, cancellationToken))
        {
            return null;
        }

        var ancestorChain = await GetAncestorChainAsync(contentItem, cancellationToken);
        var alternates = BuildDetailAlternates(contentItem, ancestorChain, languageCode, defaultLanguageCode, languages);

        return RouteResolutionOutcome.Detail(languageCode, contentItem.Id, contentType.Key.Value, contentType.DetailTemplate, alternates);
    }

    private async Task<RouteResolutionOutcome?> TryResolveListingAsync(
        string languageCode, LanguageCode languageCodeVo, string pathAfterLanguage, string defaultLanguageCode, IReadOnlyList<SiteLanguage> languages)
    {
        var contentType = await contentTypeRepository.GetByRoutePrefixAsync(languageCodeVo, pathAfterLanguage);
        if (contentType is null || !contentType.IsActive || !contentType.HasListingPage)
        {
            return null;
        }

        var translation = contentType.Translations.First(t => t.LanguageCode == languageCodeVo);

        // HasListingPage requires every translation to carry a non-empty RoutePrefix (ContentType's
        // own CheckListingPageRequiresRoutePrefix invariant), so every active language the type has a
        // translation for already has a usable listing path - no extra lookups needed.
        var alternates = languages
            .Where(l => l.IsActive && l.Code.Value != languageCode)
            .Select(l => (Language: l, Translation: contentType.Translations.FirstOrDefault(t => t.LanguageCode == l.Code)))
            .Where(x => x.Translation is not null)
            .Select(x => new RouteAlternate(
                x.Language.Code.Value, RoutePathFormat.BuildPublicPath(x.Language.Code.Value, defaultLanguageCode, x.Translation!.RoutePrefix)))
            .ToList();

        return RouteResolutionOutcome.Listing(languageCode, contentType.Key.Value, contentType.ListTemplate, translation.Name, translation.Seo, alternates);
    }

    private async Task<RouteResolutionOutcome?> TryResolveRedirectAsync(
        string languageCode, LanguageCode languageCodeVo, string pathAfterLanguage, string defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var redirect = await redirectRepository.GetByFromPathAsync(languageCodeVo, pathAfterLanguage, cancellationToken);
        if (redirect is null)
        {
            return null;
        }

        if (redirect.TargetKind == RedirectTargetKind.Path)
        {
            var targetPath = redirect.TargetPath!;
            var location = Uri.TryCreate(targetPath, UriKind.Absolute, out _)
                ? targetPath
                : RoutePathFormat.BuildPublicPath(languageCode, defaultLanguageCode, targetPath);
            return RouteResolutionOutcome.Redirect(location, (int)redirect.StatusCode, redirect.Id);
        }

        var targetItem = await contentItemRepository.GetByIdAsync(redirect.TargetContentItemId!.Value, cancellationToken);
        var targetTranslation = targetItem?.Translations.FirstOrDefault(t => t.LanguageCode == languageCodeVo);
        if (targetItem is null || targetTranslation is null || !await contentPathCascadeService.IsVisibleWithAncestorsAsync(targetItem, now, cancellationToken))
        {
            // ADR-024 §15: a redirect whose target is no longer visible is not applied - the caller
            // falls through to NotFound rather than sending visitors to a page that does not exist.
            return null;
        }

        var targetLocation = RoutePathFormat.BuildPublicPath(languageCode, defaultLanguageCode, targetTranslation.FullPath);
        return RouteResolutionOutcome.Redirect(targetLocation, (int)redirect.StatusCode, redirect.Id);
    }

    // item plus every ancestor, closest-parent-first order (item itself is not included) - fetched
    // once and reused for every candidate alternate language, rather than re-walking per language.
    private async Task<IReadOnlyList<ContentItem>> GetAncestorChainAsync(ContentItem item, CancellationToken cancellationToken)
    {
        var chain = new List<ContentItem>();
        var current = item;
        while (current.ParentId is not null)
        {
            current = await contentItemRepository.GetByIdAsync(current.ParentId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Content item '{current.ParentId}' referenced as a parent could not be found.");
            chain.Add(current);
        }

        return chain;
    }

    // ADR-024 §15: the item's own visibility (Status/PublishAtUtc/UnpublishAtUtc) does not vary by
    // language - only whether the item and every ancestor actually HAS a translation in a candidate
    // language does, since that is what a FullPath in that language requires.
    private static IReadOnlyList<RouteAlternate> BuildDetailAlternates(
        ContentItem item, IReadOnlyList<ContentItem> ancestorChain, string languageCode, string defaultLanguageCode,
        IReadOnlyList<SiteLanguage> languages)
    {
        var alternates = new List<RouteAlternate>();
        foreach (var language in languages.Where(l => l.IsActive && l.Code.Value != languageCode))
        {
            var itemTranslation = item.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
            if (itemTranslation is null || ancestorChain.Any(a => a.Translations.All(t => t.LanguageCode != language.Code)))
            {
                continue;
            }

            alternates.Add(new RouteAlternate(
                language.Code.Value, RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguageCode, itemTranslation.FullPath)));
        }

        return alternates;
    }
}
