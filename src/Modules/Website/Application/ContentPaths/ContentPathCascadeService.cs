using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.ContentPaths;

// ADR-024 §4.3 (Faz 1a Görev 4): the Application-layer half of hierarchy/path management -
// ContentPathService (Domain) does the pure depth/cycle/FullPath math; this class does the repository
// lookups that math needs (walking ancestors/descendants) and applies the result (recomputing
// descendants' FullPath, creating automatic Redirects). Every command handler that can move a
// ContentItem's path - CreateContentItem, UpdateContentItemTranslation, SetContentItemParent, and
// UpdateContentTypeTranslation's RoutePrefix cascade - calls this instead of duplicating the walk.
public sealed class ContentPathCascadeService(IContentItemRepository contentItemRepository, IRedirectRepository redirectRepository)
{
    // Validates parentId as a legal parent for itemId (pass Guid.Empty for a not-yet-created item) of
    // contentTypeId, returning the parent aggregate itself (so callers do not need a second lookup) on
    // success.
    public async Task<Result<ContentItem?>> ValidateParentAsync(
        Guid itemId, Guid contentTypeId, bool contentTypeSupportsHierarchy, Guid? parentId, CancellationToken cancellationToken)
    {
        if (parentId is null)
        {
            return Result.Success<ContentItem?>(null);
        }

        var parent = await contentItemRepository.GetByIdAsync(parentId.Value, cancellationToken);
        if (parent is null)
        {
            return Result.Failure<ContentItem?>(
                Error.NotFound("ContentItem.ParentNotFound", $"Parent content item '{parentId}' could not be found."));
        }

        var parentDepth = await ComputeDepthAsync(parent, cancellationToken);
        var descendantIds = itemId == Guid.Empty
            ? []
            : await GetDescendantIdsAsync(itemId, cancellationToken);
        var subtreeHeight = itemId == Guid.Empty
            ? 0
            : await ComputeSubtreeHeightAsync(itemId, cancellationToken);

        var validation = ContentPathService.ValidateParentAssignment(
            itemId, contentTypeId, contentTypeSupportsHierarchy, parentId, parent.ContentTypeId, parentDepth,
            descendantIds, subtreeHeight);

        return validation.IsFailure ? Result.Failure<ContentItem?>(validation.Error) : Result.Success<ContentItem?>(parent);
    }

    // Every ancestor's slug in languageCode, root-to-immediate-parent order. Empty when parent is null.
    // A missing translation partway up should not happen (SetTranslation/RemoveTranslation's own-
    // language rule guarantees a parent has every language a child has) but ends the walk defensively
    // rather than throwing if it ever does.
    public async Task<IReadOnlyList<string>> GetAncestorSlugsAsync(ContentItem? parent, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        if (parent is null)
        {
            return [];
        }

        var chain = new List<string>();
        ContentItem? current = parent;
        while (current is not null)
        {
            var translation = current.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is null)
            {
                break;
            }

            chain.Insert(0, translation.Slug);
            current = current.ParentId is null ? null : await contentItemRepository.GetByIdAsync(current.ParentId.Value, cancellationToken);
        }

        return chain;
    }

    // Recomputes every descendant's FullPath in languageCode beneath item (item's OWN translation must
    // already reflect its new path - callers update that themselves before calling this) and creates an
    // automatic Redirect for every path that moved.
    public async Task<Result> CascadeDescendantPathsAsync(
        ContentItem item, LanguageCode languageCode, string routePrefix, IReadOnlyList<string> itemAncestorSlugs,
        Guid updatedByUserId, DateTime updatedAtUtc, CancellationToken cancellationToken)
    {
        var itemTranslation = item.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (itemTranslation is null)
        {
            return Result.Success();
        }

        var childAncestorSlugs = new List<string>(itemAncestorSlugs) { itemTranslation.Slug };
        var children = await contentItemRepository.GetChildrenAsync(item.Id, cancellationToken);

        foreach (var child in children)
        {
            var childTranslation = child.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (childTranslation is null)
            {
                continue;
            }

            var oldFullPath = childTranslation.FullPath;
            child.RecomputeFullPath(languageCode, routePrefix, childAncestorSlugs, updatedByUserId, updatedAtUtc);
            var newFullPath = child.Translations.First(t => t.LanguageCode == languageCode).FullPath;

            if (oldFullPath != newFullPath)
            {
                var redirectResult = await CreateAutomaticRedirectAsync(
                    languageCode, oldFullPath, newFullPath, child.Id, updatedByUserId, updatedAtUtc, cancellationToken);
                if (redirectResult.IsFailure)
                {
                    return redirectResult;
                }
            }

            var childCascade = await CascadeDescendantPathsAsync(
                child, languageCode, routePrefix, childAncestorSlugs, updatedByUserId, updatedAtUtc, cancellationToken);
            if (childCascade.IsFailure)
            {
                return childCascade;
            }
        }

        return Result.Success();
    }

    // Records the move from oldFullPath to newFullPath for contentItemId. Automatic redirects never
    // store a target path (only the target ContentItem), so if this same item's path moves again later,
    // this earlier redirect keeps resolving correctly on its own - nothing here ever needs updating,
    // only new rows get added for each further move. Callers pass oldFullPath == newFullPath as a no-op.
    public async Task<Result> CreateAutomaticRedirectAsync(
        LanguageCode languageCode, string oldFullPath, string newFullPath, Guid contentItemId, Guid updatedByUserId,
        DateTime updatedAtUtc, CancellationToken cancellationToken)
    {
        if (oldFullPath == newFullPath)
        {
            return Result.Success();
        }

        // Real content now lives at newFullPath - any redirect that used to sit there is stale.
        // Automatic ones are silently replaced (live content always wins); a manual one blocks the
        // whole operation until an admin removes it (Görev 5's redirect management).
        var collisionCheck = await RemoveStaleRedirectAtAsync(languageCode, newFullPath, cancellationToken);
        if (collisionCheck.IsFailure)
        {
            return collisionCheck;
        }

        // oldFullPath might already have an automatic redirect from an earlier move of a DIFFERENT
        // item (or, in principle, the same item revisiting a path) - the FromPath unique index would
        // otherwise be violated, so the newer, more specific mapping wins.
        var oldPathCheck = await RemoveStaleRedirectAtAsync(languageCode, oldFullPath, cancellationToken);
        if (oldPathCheck.IsFailure)
        {
            return oldPathCheck;
        }

        var redirectResult = Redirect.CreateAutomatic(languageCode, oldFullPath, contentItemId, updatedByUserId, updatedAtUtc);
        if (redirectResult.IsFailure)
        {
            return redirectResult;
        }

        redirectRepository.Add(redirectResult.Value);

        return Result.Success();
    }

    private async Task<Result> RemoveStaleRedirectAtAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken)
    {
        var existing = await redirectRepository.GetByFromPathAsync(languageCode, fromPath, cancellationToken);
        if (existing is null)
        {
            return Result.Success();
        }

        if (!existing.IsAutomatic)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.PathCollidesWithManualRedirect",
                $"'{fromPath}' is already used by a manually-created redirect in language '{languageCode}'; remove it first."));
        }

        redirectRepository.Remove(existing);

        return Result.Success();
    }

    private async Task<int> ComputeDepthAsync(ContentItem item, CancellationToken cancellationToken)
    {
        var depth = 1;
        var current = item;
        while (current.ParentId is not null)
        {
            current = await contentItemRepository.GetByIdAsync(current.ParentId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Content item '{current.ParentId}' referenced as a parent could not be found.");
            depth++;
        }

        return depth;
    }

    private async Task<IReadOnlyCollection<Guid>> GetDescendantIdsAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var result = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(itemId);

        while (queue.Count > 0)
        {
            var children = await contentItemRepository.GetChildrenAsync(queue.Dequeue(), cancellationToken);
            foreach (var child in children)
            {
                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return result;
    }

    private async Task<int> ComputeSubtreeHeightAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var children = await contentItemRepository.GetChildrenAsync(itemId, cancellationToken);
        if (children.Count == 0)
        {
            return 0;
        }

        var maxChildHeight = 0;
        foreach (var child in children)
        {
            maxChildHeight = Math.Max(maxChildHeight, await ComputeSubtreeHeightAsync(child.Id, cancellationToken));
        }

        return 1 + maxChildHeight;
    }
}
