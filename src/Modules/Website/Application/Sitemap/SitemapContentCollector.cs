using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.RouteResolution;
using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Sitemap;

// ADR-024 §15 (Faz 5 Görev 5): assembles every sitemap entry - the home page, every active +
// HasListingPage content type's listing page, every visible + NoIndex=false detail page, and every
// external source document flagged IncludeInSitemap. Detail-page visibility deliberately reuses Görev
// 2's own ContentPathCascadeService.IsVisibleWithAncestorsAsync (the same method
// SearchIndexUpdater.ReindexItemWithKnownTypeAsync calls) instead of inlining a second copy of "self and
// every ancestor visible" - ADR-024 §15's own instruction for this task. The caller
// (GetSitemapQueryHandler) owns caching/pagination/XML rendering; this class only ever talks to
// repositories and other Application services.
public sealed class SitemapContentCollector(
    ISiteLanguageRepository siteLanguageRepository,
    IContentTypeRepository contentTypeRepository,
    IContentItemRepository contentItemRepository,
    IEventScheduleRepository eventScheduleRepository,
    ISearchDocumentRepository searchDocumentRepository,
    ContentPathCascadeService contentPathCascadeService,
    TimeProvider timeProvider)
{
    // ADR-024 §15: "yakın geçmiş (son 90 gün)" - an event whose EventSchedule.StartsAtUtc is older than
    // this many days is excluded; a future event (any distance ahead) and an event with no schedule yet
    // are both kept (there is no date to judge "old" against).
    private const int RecentEventWindowDays = 90;

    public async Task<IReadOnlyList<SitemapUrlEntry>> CollectAsync(CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        if (activeLanguages.Count == 0)
        {
            return [];
        }

        var defaultLanguage = activeLanguages.First(l => l.IsDefault);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var contentTypes = await contentTypeRepository.GetAllAsync(cancellationToken);

        var entries = new List<SitemapUrlEntry>();
        entries.AddRange(BuildHomeEntries(activeLanguages, defaultLanguage));
        entries.AddRange(BuildListingEntries(contentTypes, activeLanguages, defaultLanguage));
        entries.AddRange(await BuildContentEntriesAsync(contentTypes, activeLanguages, defaultLanguage, now, cancellationToken));
        entries.AddRange(await BuildExternalEntriesAsync(cancellationToken));

        return entries;
    }

    private static IReadOnlyList<SitemapUrlEntry> BuildHomeEntries(
        IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage defaultLanguage)
    {
        var alternates = activeLanguages
            .Select(l => new RouteAlternate(
                l.Code.Value, RoutePathFormat.BuildPublicPath(l.Code.Value, defaultLanguage.Code.Value, string.Empty)))
            .ToList();
        alternates.Add(new RouteAlternate(
            "x-default", RoutePathFormat.BuildPublicPath(defaultLanguage.Code.Value, defaultLanguage.Code.Value, string.Empty)));

        return activeLanguages
            .Select(l => new SitemapUrlEntry(
                RoutePathFormat.BuildPublicPath(l.Code.Value, defaultLanguage.Code.Value, string.Empty), LastModUtc: null, alternates))
            .ToList();
    }

    private static IReadOnlyList<SitemapUrlEntry> BuildListingEntries(
        IReadOnlyList<ContentType> contentTypes, IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage defaultLanguage)
    {
        var entries = new List<SitemapUrlEntry>();

        foreach (var contentType in contentTypes.Where(t => t.IsActive && t.HasListingPage))
        {
            var languageAlternates = new List<RouteAlternate>();
            foreach (var language in activeLanguages)
            {
                var translation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
                if (translation is null)
                {
                    continue;
                }

                languageAlternates.Add(new RouteAlternate(
                    language.Code.Value,
                    RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguage.Code.Value, translation.RoutePrefix)));
            }

            if (languageAlternates.Count == 0)
            {
                continue;
            }

            var defaultAlternate = languageAlternates.FirstOrDefault(a => a.LanguageCode == defaultLanguage.Code.Value);
            var fullAlternates = defaultAlternate is null
                ? languageAlternates
                : [..languageAlternates, new RouteAlternate("x-default", defaultAlternate.Path)];

            entries.AddRange(languageAlternates.Select(alternate => new SitemapUrlEntry(alternate.Path, LastModUtc: null, fullAlternates)));
        }

        return entries;
    }

    private async Task<IReadOnlyList<SitemapUrlEntry>> BuildContentEntriesAsync(
        IReadOnlyList<ContentType> contentTypes, IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage defaultLanguage, DateTime now,
        CancellationToken cancellationToken)
    {
        var entries = new List<SitemapUrlEntry>();
        var recentEventThreshold = now.AddDays(-RecentEventWindowDays);

        foreach (var contentType in contentTypes.Where(t => t.IsActive && t.HasDetailPage))
        {
            var roots = await contentItemRepository.GetRootItemsByContentTypeIdAsync(contentType.Id, cancellationToken);
            foreach (var root in roots)
            {
                var descendantIds = await contentPathCascadeService.GetDescendantIdsAsync(root.Id, cancellationToken);
                var candidateIds = new List<Guid> { root.Id };
                candidateIds.AddRange(descendantIds);

                foreach (var candidateId in candidateIds)
                {
                    var item = await contentItemRepository.GetByIdAsync(candidateId, cancellationToken);
                    if (item is null)
                    {
                        continue;
                    }

                    if (!await contentPathCascadeService.IsVisibleWithAncestorsAsync(item, now, cancellationToken))
                    {
                        continue;
                    }

                    if (contentType.SupportsEvent)
                    {
                        var schedule = await eventScheduleRepository.GetByContentItemIdAsync(item.Id, cancellationToken);
                        if (schedule is not null && schedule.StartsAtUtc < recentEventThreshold)
                        {
                            continue;
                        }
                    }

                    entries.AddRange(await BuildItemEntriesAsync(item, activeLanguages, defaultLanguage, cancellationToken));
                }
            }
        }

        return entries;
    }

    private async Task<IReadOnlyList<SitemapUrlEntry>> BuildItemEntriesAsync(
        ContentItem item, IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage defaultLanguage, CancellationToken cancellationToken)
    {
        var ancestorChain = await contentPathCascadeService.GetAncestorChainAsync(item, cancellationToken);
        var lastMod = item.UpdatedAtUtc ?? item.PublishedAtUtc ?? item.CreatedAtUtc;

        var alternates = new List<RouteAlternate>();
        foreach (var language in activeLanguages)
        {
            var translation = item.Translations.FirstOrDefault(t => t.LanguageCode == language.Code);
            if (translation is null || translation.Seo.NoIndex)
            {
                continue;
            }

            if (ancestorChain.Any(a => a.Translations.All(t => t.LanguageCode != language.Code)))
            {
                continue;
            }

            alternates.Add(new RouteAlternate(
                language.Code.Value, RoutePathFormat.BuildPublicPath(language.Code.Value, defaultLanguage.Code.Value, translation.FullPath)));
        }

        if (alternates.Count == 0)
        {
            return [];
        }

        var defaultAlternate = alternates.FirstOrDefault(a => a.LanguageCode == defaultLanguage.Code.Value);
        var fullAlternates = defaultAlternate is null
            ? alternates
            : [..alternates, new RouteAlternate("x-default", defaultAlternate.Path)];

        return alternates.Select(a => new SitemapUrlEntry(a.Path, lastMod, fullAlternates)).ToList();
    }

    private async Task<IReadOnlyList<SitemapUrlEntry>> BuildExternalEntriesAsync(CancellationToken cancellationToken)
    {
        var documents = await searchDocumentRepository.GetSitemapEligibleExternalAsync(
            SearchIndexUpdater.WebsiteSourceKey, cancellationToken);

        return documents
            .Select(d => new SitemapUrlEntry(d.Url, d.PublishedAtUtc, Array.Empty<RouteAlternate>()))
            .ToList();
    }
}
