namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

// BrokenLinkReason is null when Link is null (a group heading) or when the link resolves fine in the
// site's default language - non-null names why it does not (ADR-024 §1.2 "editör kırık linkleri
// görebilsin").
public sealed record MenuItemResponse(
    Guid Id,
    Guid? ParentId,
    int SortOrder,
    bool IsActive,
    MenuItemLinkResponse? Link,
    bool OpenInNewTab,
    string? IconKey,
    IReadOnlyList<MenuItemTranslationResponse> Translations,
    string? BrokenLinkReason);
