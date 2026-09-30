namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// Href is null exactly for a group heading (no LinkTarget of its own) - Görev 1's own visibility
// rules (inactive, no translation, unresolved link, or an empty group heading) already dropped
// every other case before a node reaches this shape.
public sealed record PublicMenuItemResponse(
    string Label, string? Href, bool OpenInNewTab, string? IconKey, IReadOnlyList<PublicMenuItemResponse> Children);
