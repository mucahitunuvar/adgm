using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 1 master prompt §1.2: one Menu aggregate per MenuLocation (migration-seeded, never
// created/deleted by an admin action - only ReplaceItems mutates one). MenuItem is a child entity:
// the whole tree is replaced atomically in one call (the editor's drag-and-drop UI saves the entire
// tree at once), never edited node-by-node - the same "whole-collection replace" shape
// SetContentItemGallery/SetContentItemCategories already use for a flat child collection, extended
// here with the tree-shaped validation (cycles, depth, per-location single-level rule) only a real
// hierarchy needs.
public sealed class Menu : AggregateRoot
{
    public const int MaxItems = 200;
    public const int MaxDepth = 3;

    private readonly List<MenuItem> _items = [];

    public MenuLocation Location { get; private set; }

    public IReadOnlyList<MenuItem> Items => _items.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Menu(Guid id, MenuLocation location, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Location = location;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Menu()
    {
    }

    public static Menu CreateEmpty(MenuLocation location, Guid createdByUserId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), location, createdByUserId, createdAtUtc);

    // Replaces the entire item tree at once. `items` must already be fully constructed (MenuItem.Create
    // has validated each item's own fields) with real, stable ids - the Application-layer command
    // handler is the one that resolves the request's client-supplied temporary ids into these.
    public Result ReplaceItems(IReadOnlyList<MenuItem> items, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (items.Count > MaxItems)
        {
            return Result.Failure(Error.Validation("Menu.TooManyItems", $"A menu can have at most {MaxItems} items."));
        }

        var duplicateId = items.GroupBy(i => i.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicateId is not null)
        {
            return Result.Failure(Error.Validation("Menu.DuplicateItemId", $"Menu item id '{duplicateId.Key}' appears more than once."));
        }

        var itemsById = items.ToDictionary(i => i.Id);
        foreach (var item in items)
        {
            if (item.ParentId is { } parentId && !itemsById.ContainsKey(parentId))
            {
                return Result.Failure(Error.Validation(
                    "Menu.ParentNotFound", $"Menu item '{item.Id}' references a parent that is not part of this update."));
            }
        }

        if (Location == MenuLocation.Utility && items.Any(i => i.ParentId is not null))
        {
            return Result.Failure(Error.Validation("Menu.UtilityMustBeSingleLevel", "The Utility menu cannot have nested items."));
        }

        foreach (var item in items)
        {
            var depthCheck = ValidateDepthAndCycles(item, itemsById);
            if (depthCheck.IsFailure)
            {
                return depthCheck;
            }
        }

        _items.Clear();
        _items.AddRange(items);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private static Result ValidateDepthAndCycles(MenuItem item, IReadOnlyDictionary<Guid, MenuItem> itemsById)
    {
        var visited = new HashSet<Guid> { item.Id };
        var depth = 1;
        var current = item;

        while (current.ParentId is { } parentId)
        {
            if (!visited.Add(parentId))
            {
                return Result.Failure(Error.Validation("Menu.CyclicReference", "Menu items must not form a cycle."));
            }

            current = itemsById[parentId];
            depth++;

            if (depth > MaxDepth)
            {
                return Result.Failure(Error.Validation(
                    "Menu.MaxDepthExceeded", $"Menu items can be nested at most {MaxDepth} levels deep."));
            }
        }

        return Result.Success();
    }

    // ADR-024 §1.2 (Faz 2 Görev 1) "kullanım koruması": called when a ContentItem this menu links to
    // is permanently deleted - the link is cleared and the item deactivated in place (it does not
    // silently turn into an active-but-empty group heading), leaving every other item untouched.
    public bool DeactivateItemsLinkingToContent(Guid contentItemId, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var affected = false;
        foreach (var item in _items.Where(i => i.LinkTarget.Kind == LinkTargetKind.Content && i.LinkTarget.ContentItemId == contentItemId))
        {
            item.ClearLinkAndDeactivate();
            affected = true;
        }

        if (affected)
        {
            Touch(updatedByUserId, updatedAtUtc);
        }

        return affected;
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
