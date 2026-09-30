namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

// TempId/ParentTempId are client-assigned (the editor's drag-and-drop tree has no real ids for
// brand-new nodes) - the command handler resolves them into real, stable Guids and rebuilds
// parent/child relationships from that mapping (§1.2 "Öğeler istemcide geçici ID taşıyabilir").
public sealed record MenuItemTreeInput(
    string TempId,
    string? ParentTempId,
    int SortOrder,
    bool IsActive,
    MenuItemLinkInput? Link,
    bool OpenInNewTab,
    string? IconKey,
    IReadOnlyList<MenuItemTranslationInput> Translations);
