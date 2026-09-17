namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

// SearchText matches FirstName/LastName/Email as a case-insensitive substring.
public sealed record CandidateCvSearchFilter(string? SearchText, int Page, int PageSize);
