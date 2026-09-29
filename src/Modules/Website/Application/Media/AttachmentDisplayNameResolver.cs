namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §4.1 (Faz 1b Görev 4): public attachment display name resolution - the content item's own
// override for the requested language, then the MediaAsset's original file name. Introduced now,
// alongside the attachment relationship itself, for the same reason as GalleryItemDisplayResolver.
public static class AttachmentDisplayNameResolver
{
    public static string Resolve(string? displayNameOverride, string originalFileName) =>
        !string.IsNullOrWhiteSpace(displayNameOverride) ? displayNameOverride : originalFileName;
}
