namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

public sealed record ContentPreviewRelatedItemResponse(Guid Id, string Title, string Summary, string? CoverImageUrl);
