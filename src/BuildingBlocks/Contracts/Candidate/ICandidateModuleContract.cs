namespace GenclikMerkezi.Contracts.Candidate;

// The published, in-process contract other modules depend on instead of Candidate's own DbContext
// (ADR-016 Decision 2, Option C - same pattern as ICompanyModuleContract/ICareerAdvisorModuleContract).
// Implemented in Candidate.Infrastructure, registered once at the Host composition root. Candidate's
// first public contract - kept deliberately minimal for its first consumer (Matching, Görev 7:
// CandidateSuggestion needs the suggested candidate's display name and owning advisor).
public interface ICandidateModuleContract
{
    Task<CandidateCvSummary?> GetCandidateCvByIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    Task<Guid?> GetCareerAdvisorIdForCandidateAsync(Guid candidateCvId, CancellationToken cancellationToken = default);
}
