using GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;
using GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Features.ReassignOrphanedCompanies;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Admin;

// Host-seviyesi orkestrasyon (ADR-022 §1): danışman deaktivasyonu ve yeniden atama, üç modülün de
// dışında, sıralı komutlarla yürütülür - modüller birbirini hiç bilmiyor. CareerAdvisor →
// Candidate/Employer doğrudan çağrısı bir modül bağımlılık döngüsü oluştururdu (ModuleBoundaryTests
// ihlali); bu sınıf yalnızca Host'un (bir "modül" olmayan, ModuleBoundaryTests kapsamı dışındaki
// composition root) her üç modülün Features namespace'ini de bilmesine izin vermesinden yararlanıyor.
public sealed class CareerAdvisorDeactivationOrchestrator(ISender sender)
{
    public async Task<Result> DeactivateAndReassignAsync(Guid careerAdvisorId, CancellationToken cancellationToken)
    {
        var deactivateResult = await sender.Send(new DeactivateCareerAdvisorCommand(careerAdvisorId), cancellationToken);

        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        var reassignCandidatesResult = await sender.Send(new ReassignOrphanedCandidatesCommand(careerAdvisorId), cancellationToken);

        if (reassignCandidatesResult.IsFailure)
        {
            return Result.Failure(reassignCandidatesResult.Error);
        }

        // İki reassign komutu birbirinden bağımsız modüllerde (Candidate/Employer) olduğu için
        // sırası önemli değil - ikisi de yalnızca deactivate'ten SONRA çalışmalı.
        var reassignCompaniesResult = await sender.Send(new ReassignOrphanedCompaniesCommand(careerAdvisorId), cancellationToken);

        return reassignCompaniesResult.IsFailure ? Result.Failure(reassignCompaniesResult.Error) : Result.Success();
    }
}
