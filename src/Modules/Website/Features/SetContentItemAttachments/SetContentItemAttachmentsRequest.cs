namespace GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;

public sealed record SetContentItemAttachmentsRequest(byte[] RowVersion, IReadOnlyList<AttachmentInput> Items);
