namespace GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;

public sealed record AttachmentInput(Guid MediaAssetId, int SortOrder, IReadOnlyList<AttachmentTranslationInput> Translations);
