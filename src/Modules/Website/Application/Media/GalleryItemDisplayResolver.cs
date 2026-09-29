namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §4.1 (Faz 1b Görev 4): public gallery alt text/caption resolution chain - the content
// item's own override for the requested language, then the MediaAsset's own translation for that
// language, then an empty string. Never falls back to the content item's title (a wrong alt text is
// worse for screen readers than an empty one). Introduced now, alongside the gallery relationship
// itself, even though Görev 7's public detail/list response is its first real caller - the same
// "port ships with the relationship, wired up by a later Görev" pattern IVideoUsageChecker used in
// Görev 2/4.
public static class GalleryItemDisplayResolver
{
    public static string ResolveAltText(string? altTextOverride, string? mediaAssetAltText) =>
        !string.IsNullOrWhiteSpace(altTextOverride) ? altTextOverride : mediaAssetAltText ?? string.Empty;

    public static string ResolveCaption(string? captionOverride, string? mediaAssetCaption) =>
        !string.IsNullOrWhiteSpace(captionOverride) ? captionOverride : mediaAssetCaption ?? string.Empty;
}
