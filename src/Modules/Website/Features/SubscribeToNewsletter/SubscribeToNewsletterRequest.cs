namespace GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;

public sealed record SubscribeToNewsletterRequest(
    string? SubmissionToken, string? TurnstileToken, string? Website, string? Email, string? Lang, int? AcceptedPrivacyNoticeVersion);
