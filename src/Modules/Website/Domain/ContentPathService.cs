using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.3 (Faz 1a Görev 4): path/hierarchy computation that spans more than one ContentItem
// aggregate (AGENTS.md §10 - this behavior does not belong to a single aggregate). A genuine Domain
// service, not an Application-layer helper: every input here is a plain value or an already-resolved
// collection the caller supplies, never a repository - Domain must not depend on
// Application.Abstractions' repository interfaces (that would invert the dependency direction Clean
// Architecture requires). The Application-layer command handlers that call this do the actual
// repository lookups (walking descendants, fetching ancestor slugs) and apply whatever this service
// decides.
public static class ContentPathService
{
    public const int MaxHierarchyDepth = 3;

    // itemDescendantIds/itemSubtreeHeight describe the item BEING (re)parented, not the candidate
    // parent - an item can never become a descendant of its own descendant (a cycle), and moving a
    // subtree must not push any of its own descendants past MaxHierarchyDepth.
    public static Result ValidateParentAssignment(
        Guid itemId,
        Guid itemContentTypeId,
        bool contentTypeSupportsHierarchy,
        Guid? parentId,
        Guid? parentContentTypeId,
        int parentDepth,
        IReadOnlyCollection<Guid> itemDescendantIds,
        int itemSubtreeHeight)
    {
        if (parentId is null)
        {
            return Result.Success();
        }

        if (!contentTypeSupportsHierarchy)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.HierarchyNotSupported", "The content type does not support a parent/child hierarchy."));
        }

        if (parentId == itemId || itemDescendantIds.Contains(parentId.Value))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.CyclicHierarchy", "A content item cannot become its own descendant."));
        }

        if (parentContentTypeId != itemContentTypeId)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ParentMustBeSameContentType", "The parent must be of the same content type."));
        }

        if (parentDepth + 1 + itemSubtreeHeight > MaxHierarchyDepth)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.MaxHierarchyDepthExceeded", $"Maximum hierarchy depth is {MaxHierarchyDepth}."));
        }

        return Result.Success();
    }

    // routePrefix is the owning ContentType's RoutePrefix for the language in question (empty for a
    // root-level type); ancestorSlugsRootToParent is every ancestor's slug in that same language, in
    // root-to-immediate-parent order (empty for a root-level item, regardless of type).
    // ADR-024 §4.3 Görev 4: "içerik ancak kendisi ve tüm ataları görünürse görünür" - a scheduled or
    // unpublished ancestor hides everything beneath it even if the descendant's own IsVisible(now) is
    // true. The actual public query that gathers ancestorVisibilities is Görev 6's; this one-line rule
    // is defined here now so Görev 6 has a single source to call instead of re-deriving it.
    public static bool IsVisibleConsideringAncestors(bool selfVisible, IEnumerable<bool> ancestorVisibilities) =>
        selfVisible && ancestorVisibilities.All(visible => visible);

    public static string ComputeFullPath(string routePrefix, IReadOnlyList<string> ancestorSlugsRootToParent, string ownSlug)
    {
        var segmentCount = ancestorSlugsRootToParent.Count + 1 + (routePrefix.Length == 0 ? 0 : 1);
        var segments = new List<string>(segmentCount);

        if (routePrefix.Length > 0)
        {
            segments.Add(routePrefix);
        }

        segments.AddRange(ancestorSlugsRootToParent);
        segments.Add(ownSlug);

        return string.Join('/', segments);
    }
}
