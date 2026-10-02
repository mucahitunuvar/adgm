namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: shared by the content-list and upcoming-events blocks' `data`
// ("öğe biçimi public liste öğesiyle aynı" - the same core fields GetPublicContents' own list item
// already carries: Path for a HasDetailPage type, or Body/DetailImage inline for one without).
public sealed record PublicContentListBlockItemResponse(
    Guid Id,
    string Title,
    string Summary,
    string? Path,
    PublicBlockImageResponse? CoverImage,
    DateTime EffectivePublishDate,
    bool IsFeatured,
    string? Body,
    PublicBlockImageResponse? DetailImage);
