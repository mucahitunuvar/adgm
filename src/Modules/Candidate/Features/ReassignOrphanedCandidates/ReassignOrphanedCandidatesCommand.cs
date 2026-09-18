using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;

// Endpoint'siz komut (Identity'nin CreateStaffUserCommand'ı ile aynı desen) - yalnızca Host
// katmanındaki CareerAdvisorDeactivationOrchestrator'dan ISender ile çağrılır (Görev 3/ADR-022 §1).
public sealed record ReassignOrphanedCandidatesCommand(Guid CareerAdvisorId)
    : IRequest<Result<ReassignOrphanedCandidatesResponse>>;
