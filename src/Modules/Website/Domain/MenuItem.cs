using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 1 master prompt §1.2: one node of a Menu's item tree. ParentId is a plain scalar
// reference to another MenuItem's Id within the SAME Menu (the ContentCategory/ContentItem
// convention for a self-referencing hierarchy - no EF navigation, the tree is built/validated by
// walking this column in memory: see Menu.ReplaceItems). An item with LinkTarget.IsEmpty is a group
// heading with no link of its own (§1.2: "yoksa öğe grup başlığıdır").
public sealed partial class MenuItem : Entity
{
    public const int MaxIconKeyLength = 50;

    private readonly List<MenuItemTranslation> _translations = [];

    public Guid? ParentId { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public LinkTarget LinkTarget { get; private set; } = LinkTarget.CreateEmpty();

    public bool OpenInNewTab { get; private set; }

    public string? IconKey { get; private set; }

    public IReadOnlyList<MenuItemTranslation> Translations => _translations.AsReadOnly();

    private MenuItem(
        Guid id, Guid? parentId, int sortOrder, bool isActive, LinkTarget linkTarget, bool openInNewTab, string? iconKey)
        : base(id)
    {
        ParentId = parentId;
        SortOrder = sortOrder;
        IsActive = isActive;
        LinkTarget = linkTarget;
        OpenInNewTab = openInNewTab;
        IconKey = iconKey;
    }

    private MenuItem()
    {
    }

    public static Result<MenuItem> Create(
        Guid id,
        Guid? parentId,
        int sortOrder,
        bool isActive,
        LinkTarget linkTarget,
        bool openInNewTab,
        string? iconKey,
        IReadOnlyList<MenuItemTranslation> translations)
    {
        var normalizedIconKey = string.IsNullOrWhiteSpace(iconKey) ? null : iconKey.Trim();
        if (normalizedIconKey is not null
            && (normalizedIconKey.Length > MaxIconKeyLength || !IconKeyPattern().IsMatch(normalizedIconKey)))
        {
            return Result.Failure<MenuItem>(Error.Validation(
                "MenuItem.IconKeyInvalid", $"Icon key must match '[a-z0-9-]' and be at most {MaxIconKeyLength} characters."));
        }

        if (translations.Count == 0)
        {
            return Result.Failure<MenuItem>(Error.Validation("MenuItem.TranslationsRequired", "A menu item requires at least one translation."));
        }

        var item = new MenuItem(id, parentId, sortOrder, isActive, linkTarget, openInNewTab, normalizedIconKey);
        item._translations.AddRange(translations);

        return Result.Success(item);
    }

    // ADR-024 §1.2 (Faz 2 Görev 1): Menu.DeactivateItemsLinkingToContent's per-item effect - internal
    // since only the owning Menu aggregate may reach into a child entity like this.
    internal void ClearLinkAndDeactivate()
    {
        LinkTarget = LinkTarget.CreateEmpty();
        IsActive = false;
    }

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex IconKeyPattern();
}
