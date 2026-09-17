namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

// Projection shape for the admin/advisor-facing candidate list - deliberately not the full
// CandidateCv aggregate (AGENTS.md §40: avoid loading entire aggregates for simple read projections).
public sealed record CandidateCvSummary(
    Guid Id, Guid UserId, string FirstName, string LastName, string Email, int CompletionPercentage);
