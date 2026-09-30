namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

// ADR-024 §17 (Faz 1b Görev 7): Path/Body/DetailImage/Attachments are mutually exclusive by
// ContentType.HasDetailPage - a type with its own detail page gets Path (null Body/DetailImage/
// Attachments); a type without one (FAQ, Team, Document) gets its content inline here instead
// (null Path).
public sealed record PublicContentListItemResponse(
    Guid Id,
    string Title,
    string Summary,
    string? Path,
    PublicContentImageResponse? CoverImage,
    DateTime EffectivePublishDate,
    bool IsFeatured,
    IReadOnlyList<PublicContentCategoryResponse> Categories,
    string? Body,
    PublicContentImageResponse? DetailImage,
    IReadOnlyList<PublicContentAttachmentResponse>? Attachments);
