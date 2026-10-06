namespace GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;

public sealed record ResendEventRegistrationVerificationRequest(
    Guid ContentItemId, string? SubmissionToken, string? TurnstileToken, string? Website, string? Email);
