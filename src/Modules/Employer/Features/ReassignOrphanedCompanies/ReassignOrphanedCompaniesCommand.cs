using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.ReassignOrphanedCompanies;

// Endpoint'siz komut (Candidate.Features.ReassignOrphanedCandidatesCommand ile aynı desen) - yalnızca
// Host katmanındaki CareerAdvisorDeactivationOrchestrator'dan ISender ile çağrılır.
public sealed record ReassignOrphanedCompaniesCommand(Guid CareerAdvisorId) : IRequest<Result<ReassignOrphanedCompaniesResponse>>;
