namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record FormDefinitionDetailResponse(
    Guid Id,
    string Key,
    bool IsActive,
    int RetentionDays,
    IReadOnlyList<string> NotificationEmails,
    string PrivacyNoticeKey,
    IReadOnlyList<FormExplicitConsentResponse> ExplicitConsents,
    int DefinitionVersion,
    byte[] RowVersion,
    DateTime CreatedAtUtc,
    IReadOnlyList<FormDefinitionTranslationResponse> Translations,
    IReadOnlyList<FormFieldResponse> Fields);
