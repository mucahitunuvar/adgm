namespace GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;

public sealed record CreateEventRegistrationRequest(
    string? SubmissionToken,
    string? TurnstileToken,
    string? Website,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Lang,
    int? AcceptedPrivacyNoticeVersion);
