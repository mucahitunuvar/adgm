namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinition;

public sealed record UpdateFormDefinitionRequest(
    byte[] RowVersion,
    int? RetentionDays,
    IReadOnlyList<string> NotificationEmails,
    string? PrivacyNoticeKey,
    IReadOnlyList<UpdateFormDefinitionExplicitConsentInput> ExplicitConsents);
