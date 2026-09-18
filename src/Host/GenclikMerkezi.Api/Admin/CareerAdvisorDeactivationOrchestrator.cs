using GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;
using GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Admin;

// Host-seviyesi orkestrasyon (ADR-022 §1): danışman deaktivasyonu ve yeniden atama, iki modülün de
// dışında, sıralı iki komutla yürütülür - iki modül birbirini hiç bilmiyor. CareerAdvisor →
// Candidate/Employer doğrudan çağrısı bir modül bağımlılık döngüsü oluştururdu (ModuleBoundaryTests
// ihlali); bu sınıf yalnızca Host'un (bir "modül" olmayan, ModuleBoundaryTests kapsamı dışındaki
// composition root) her iki modülün Features namespace'ini de bilmesine izin vermesinden yararlanıyor.
public sealed class CareerAdvisorDeactivationOrchestrator(ISender sender)
{
    public async Task<Result> DeactivateAndReassignAsync(Guid careerAdvisorId, CancellationToken cancellationToken)
    {
        var deactivateResult = await sender.Send(new DeactivateCareerAdvisorCommand(careerAdvisorId), cancellationToken);

        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        var reassignResult = await sender.Send(new ReassignOrphanedCandidatesCommand(careerAdvisorId), cancellationToken);

        return reassignResult.IsFailure ? Result.Failure(reassignResult.Error) : Result.Success();
    }
}
