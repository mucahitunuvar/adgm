namespace GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;

public sealed record PublishLegalDocumentDraftRequest(byte[] RowVersion, DateTime? EffectiveAtUtc);
