using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §17 (Faz 1b Görev 7): GetPublicContentsQueryHandler's projection shape - only the requested
// language's display fields, never the full translation collection. Body/DetailImageMediaId are
// always projected (cheap - more columns in the same row, not another round trip) even though only
// HasDetailPage = false types end up using them.
public sealed record PublicContentListItemCandidate(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string FullPath,
    Guid? CoverImageMediaId,
    Guid? DetailImageMediaId,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    DateTime EffectivePublishDate,
    bool IsFeatured,
    IReadOnlyList<Guid> CategoryIds,
    SeoMetadata Seo);
