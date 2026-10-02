namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: video-feature's `data` - Href is null when Texts carried no link
// or it did not resolve, in which case the frontend hides only the button, not the whole block
// ("tekil link'lerde ilgili buton ... gizlenir").
public sealed record PublicVideoFeatureBlockDataResponse(string EmbedUrl, string ThumbnailUrl, string? Href);
