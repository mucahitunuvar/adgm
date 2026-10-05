namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §14 (Faz 3 Görev 6): the admin list's projection - "liste kişisel veri içermez" does not
// apply here (unlike GetFormSubmissions' own list item), since the subscriber's email IS the whole
// point of this list; that is exactly why GetNewsletterSubscribersEndpoint records a PersonalDataAccessLog
// entry that GetFormSubmissionsEndpoint does not.
public sealed record NewsletterSubscriberListItem(
    Guid Id, string Email, string LanguageCode, string Status, DateTime SubscribedAtUtc, DateTime? ConfirmedAtUtc);
