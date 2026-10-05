using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.3 (Faz 3 Görev 1): signed, time-limited submission tokens that prove an anonymous
// write (form submission, newsletter subscription, cookie consent) was preceded by a real page load -
// GET /api/v1/public/submission-tokens issues one, and IPublicSubmissionGuard checks both that it is
// still valid AND that enough time (ReservedFillDuration) has passed since it was issued. The issued
// time travels inside the signed payload itself, never in a client-supplied field - the client cannot
// be trusted to report its own elapsed time. Infrastructure implements this with ASP.NET Core Data
// Protection, the same approach IContentPreviewLinkGenerator already uses.
public interface ISubmissionTokenGenerator
{
    string GenerateToken(DateTimeOffset issuedAtUtc, TimeSpan duration);

    // A single failure mode ("PublicSubmission.InvalidToken") covers a malformed, tampered-with AND an
    // expired token alike - IPublicSubmissionGuard turns any failure into the same ayrıntı vermeyen
    // rejection the master prompt requires, so the two cases need no distinction here.
    Result<DateTimeOffset> ValidateToken(string? token);
}
