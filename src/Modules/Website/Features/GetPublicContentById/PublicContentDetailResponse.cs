namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record PublicContentDetailResponse(
    Guid Id,
    string ContentTypeKey,
    string DetailTemplate,
    string Title,
    string Summary,
    string Body,
    string Path,
    DateTime EffectivePublishDate,
    DateTime? UpdatedAtUtc,
    PublicContentDetailImageResponse? CoverImage,
    PublicContentDetailImageResponse? DetailImage,
    IReadOnlyList<PublicContentDetailGalleryItemResponse> Gallery,
    IReadOnlyList<PublicContentDetailVideoResponse> Videos,
    IReadOnlyList<PublicContentDetailAttachmentResponse> Attachments,
    IReadOnlyList<PublicContentDetailCategoryResponse> Categories,
    IReadOnlyList<string> Tags,
    IReadOnlyList<PublicContentChildResponse> Children,
    IReadOnlyList<PublicContentRelatedItemResponse> Related,
    IReadOnlyList<PublicContentBreadcrumbItemResponse> Breadcrumb,
    IReadOnlyList<PublicContentDetailAlternateResponse> Alternates,
    PublicContentDetailSeoResponse Seo);
