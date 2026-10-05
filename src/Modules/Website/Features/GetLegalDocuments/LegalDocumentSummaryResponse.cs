namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocuments;

public sealed record LegalDocumentSummaryResponse(
    Guid Id, string Key, string Kind, string Title, int? EffectiveVersionNumber, bool HasDraft, byte[] RowVersion);
