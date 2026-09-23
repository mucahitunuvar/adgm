namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// One place a MediaAsset is referenced from (a content item, a block, a slider, SiteSettings' logo,
// etc.) - Description is a short human-readable label ("Site ayarları - Logo"), Url an optional deep
// link into wherever it's edited. Later Faz aggregates each contribute their own IMediaUsageProvider
// rather than MediaAsset knowing about them.
public sealed record MediaUsage(string SourceKey, Guid SourceId, string Description, string? Url);
