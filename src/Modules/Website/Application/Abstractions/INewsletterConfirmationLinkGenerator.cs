using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §14 (Faz 3 Görev 6): the double opt-in confirmation link - signed and time-limited (7 days),
// the same Data Protection approach ISubmissionTokenGenerator/IContentPreviewLinkGenerator already use,
// but carrying a subscriber id as its payload instead of an issued time or a content item id.
// Deliberately a separate abstraction from the unsubscribe link: that one is never signed/expiring
// (NewsletterSubscriber.UnsubscribeToken's own remarks explain why), so it needs no generator here.
public interface INewsletterConfirmationLinkGenerator
{
    string GenerateToken(Guid subscriberId, TimeSpan duration);

    // A single failure mode covers a malformed, tampered-with AND an expired token alike - callers
    // never need to distinguish why a confirmation link no longer works.
    Result<Guid> ValidateToken(string? token);
}
