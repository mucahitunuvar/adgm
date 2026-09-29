namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed record ContentItemAttachmentResponse(
    Guid MediaAssetId, int SortOrder, IReadOnlyList<ContentItemAttachmentTranslationResponse> Translations);
