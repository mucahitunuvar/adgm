namespace GenclikMerkezi.Modules.Website.Features.GetNewsletterSubscribers;

public sealed record NewsletterSubscriberSummaryResponse(
    Guid Id, string Email, string LanguageCode, string Status, DateTime SubscribedAtUtc, DateTime? ConfirmedAtUtc);
