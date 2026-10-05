namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;

public sealed record LegalDocumentDetailResponse(
    Guid Id,
    string Key,
    string Kind,
    byte[] RowVersion,
    DateTime CreatedAtUtc,
    int? EffectiveVersionNumber,
    IReadOnlyList<LegalDocumentTranslationResponse> Translations,
    IReadOnlyList<LegalDocumentVersionResponse> Versions);
