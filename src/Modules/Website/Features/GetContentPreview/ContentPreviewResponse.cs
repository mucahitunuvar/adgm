namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

// ADR-024 §4.5 (Faz 1b Görev 6): a reasonably complete detail view for the token's target - title,
// body, images, gallery, videos, attachments, categories, tags and related content. Breadcrumb,
// hreflang alternates and child-page listing are Görev 7 concerns (they need the public listing/
// detail infrastructure that Görev builds) and are deliberately left out here; Görev 7's public detail
// response is expected to reconcile with (and likely subsume) this shape.
public sealed record ContentPreviewResponse(
    Guid Id,
    string ContentTypeKey,
    string DetailTemplate,
    string Title,
    string Summary,
    string Body,
    ContentPreviewImageResponse? CoverImage,
    ContentPreviewImageResponse? DetailImage,
    IReadOnlyList<ContentPreviewGalleryItemResponse> Gallery,
    IReadOnlyList<ContentPreviewVideoResponse> Videos,
    IReadOnlyList<ContentPreviewAttachmentResponse> Attachments,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ContentPreviewRelatedItemResponse> Related);
