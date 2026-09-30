using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.LinkTargets;

// Faz 2 Görev 1 master prompt §1.1: the single Application-layer service that turns a LinkTarget into
// a public href, shared by every future consumer (Menu now; Slider/Popup in Görev 2/6, content-list
// blocks in Görev 5). ResolveManyAsync is the batched entry point every caller with more than one
// target should use - resolving every Content link in a whole menu costs O(hierarchy depth) content
// item queries (LoadContentItemsWithAncestorsAsync) plus one ContentType query, never one query per
// link (§1.1 "N+1 yok").
public sealed class LinkTargetResolver(IContentItemRepository contentItemRepository, IContentTypeRepository contentTypeRepository)
{
    public async Task<IReadOnlyDictionary<LinkTarget, LinkTargetResolution>> ResolveManyAsync(
        IReadOnlyCollection<LinkTarget> targets,
        LanguageCode languageCode,
        LanguageCode defaultLanguageCode,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var distinctTargets = targets.Where(t => !t.IsEmpty).Distinct().ToList();
        var result = new Dictionary<LinkTarget, LinkTargetResolution>();

        if (distinctTargets.Count == 0)
        {
            return result;
        }

        var contentTargets = distinctTargets.Where(t => t.Kind == LinkTargetKind.Content).ToList();
        var contentTypeTargets = distinctTargets.Where(t => t.Kind == LinkTargetKind.ContentTypeListing).ToList();
        var pathAndUrlTargets = distinctTargets.Where(t => t.Kind is LinkTargetKind.InternalPath or LinkTargetKind.ExternalUrl).ToList();

        var contentTypesById = contentTargets.Count > 0 || contentTypeTargets.Count > 0
            ? (await contentTypeRepository.GetAllAsync(cancellationToken)).ToDictionary(t => t.Id)
            : new Dictionary<Guid, ContentType>();

        if (contentTargets.Count > 0)
        {
            var itemsById = await LoadContentItemsWithAncestorsAsync(
                contentTargets.Select(t => t.ContentItemId!.Value).ToList(), cancellationToken);

            foreach (var target in contentTargets)
            {
                result[target] = ResolveContent(target, itemsById, contentTypesById, languageCode, defaultLanguageCode, now);
            }
        }

        foreach (var target in contentTypeTargets)
        {
            result[target] = ResolveContentTypeListing(target, contentTypesById, languageCode, defaultLanguageCode);
        }

        foreach (var target in pathAndUrlTargets)
        {
            result[target] = target.Kind == LinkTargetKind.InternalPath
                ? LinkTargetResolution.Resolved(
                    RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, target.InternalPath!.TrimStart('/')))
                : LinkTargetResolution.Resolved(target.ExternalUrl!);
        }

        return result;
    }

    public async Task<LinkTargetResolution> ResolveAsync(
        LinkTarget target, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken = default)
    {
        if (target.IsEmpty)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.None);
        }

        var resolved = await ResolveManyAsync([target], languageCode, defaultLanguageCode, now, cancellationToken);
        return resolved[target];
    }

    // Fetches `ids` and every ancestor still missing, one batch query per hierarchy level, until the
    // closure is complete - bounded by the content hierarchy's max depth, never by how many ids were
    // requested.
    private async Task<IReadOnlyDictionary<Guid, ContentItem>> LoadContentItemsWithAncestorsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var itemsById = new Dictionary<Guid, ContentItem>();
        var pending = new HashSet<Guid>(ids);

        while (pending.Count > 0)
        {
            var batch = await contentItemRepository.GetByIdsAsync(pending, cancellationToken);
            pending.Clear();

            foreach (var item in batch)
            {
                itemsById[item.Id] = item;
                if (item.ParentId is { } parentId && !itemsById.ContainsKey(parentId))
                {
                    pending.Add(parentId);
                }
            }
        }

        return itemsById;
    }

    private static LinkTargetResolution ResolveContent(
        LinkTarget target,
        IReadOnlyDictionary<Guid, ContentItem> itemsById,
        IReadOnlyDictionary<Guid, ContentType> contentTypesById,
        LanguageCode languageCode,
        LanguageCode defaultLanguageCode,
        DateTime now)
    {
        if (!itemsById.TryGetValue(target.ContentItemId!.Value, out var item))
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentNotFound);
        }

        if (!contentTypesById.TryGetValue(item.ContentTypeId, out var contentType) || !contentType.IsActive)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTypeInactive);
        }

        if (!contentType.HasDetailPage)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTypeHasNoDetailPage);
        }

        var current = item;
        while (true)
        {
            if (!current.IsVisible(now))
            {
                return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentNotVisible);
            }

            if (current.ParentId is null)
            {
                break;
            }

            if (!itemsById.TryGetValue(current.ParentId.Value, out current!))
            {
                return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentNotFound);
            }
        }

        var translation = item.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTranslationMissing);
        }

        return LinkTargetResolution.Resolved(RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, translation.FullPath));
    }

    private static LinkTargetResolution ResolveContentTypeListing(
        LinkTarget target, IReadOnlyDictionary<Guid, ContentType> contentTypesById, LanguageCode languageCode, LanguageCode defaultLanguageCode)
    {
        if (!contentTypesById.TryGetValue(target.ContentTypeId!.Value, out var contentType) || !contentType.IsActive)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTypeNotFound);
        }

        if (!contentType.HasListingPage)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTypeHasNoListingPage);
        }

        var translation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
        {
            return LinkTargetResolution.Unresolved(LinkTargetUnresolvedReason.ContentTypeTranslationMissing);
        }

        return LinkTargetResolution.Resolved(
            RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, translation.RoutePrefix));
    }
}
