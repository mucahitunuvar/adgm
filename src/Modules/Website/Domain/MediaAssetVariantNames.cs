namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 0 Görev 5 fixes the exact widths ADR-024 §6 left as a placeholder: small=400, medium=800,
// large=1600 (WebP, quality 82) - see SkiaSharpImageProcessor for where these widths are used.
public static class MediaAssetVariantNames
{
    public const string Small = "small";
    public const string Medium = "medium";
    public const string Large = "large";
}
