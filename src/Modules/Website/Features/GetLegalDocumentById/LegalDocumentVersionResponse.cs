namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;

public sealed record LegalDocumentVersionResponse(
    Guid Id,
    int VersionNumber,
    string Status,
    DateTime? EffectiveAtUtc,
    DateTime? PublishedAtUtc,
    Guid? PublishedByUserId,
    string? ChangeSummary,
    IReadOnlyList<LegalDocumentVersionTranslationResponse> Translations);
