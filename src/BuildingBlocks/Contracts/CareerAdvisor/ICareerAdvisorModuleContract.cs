namespace GenclikMerkezi.Contracts.CareerAdvisor;

// The published, in-process contract other modules depend on instead of CareerAdvisor's own
// DbContext (same pattern as IIdentityService / IReferenceDataLookupReader - ADR-016 Decision 2,
// Option C). Implemented in CareerAdvisor.Infrastructure, registered once at the Host composition
// root. Kept deliberately minimal: consuming modules (Candidate/Employer) compute least-loaded
// assignment themselves from their own CareerAdvisorId groupings, so only the active advisor id
// list is exposed here, not workload counts.
public interface ICareerAdvisorModuleContract
{
    Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(CancellationToken cancellationToken = default);
}
