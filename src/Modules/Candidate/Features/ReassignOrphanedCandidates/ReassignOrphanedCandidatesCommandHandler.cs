using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;

public sealed class ReassignOrphanedCandidatesCommandHandler(
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateCvRepository candidateCvRepository,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReassignOrphanedCandidatesCommand, Result<ReassignOrphanedCandidatesResponse>>
{
    public async Task<Result<ReassignOrphanedCandidatesResponse>> Handle(
        ReassignOrphanedCandidatesCommand request, CancellationToken cancellationToken)
    {
        var orphanedCandidates = await candidateCvRepository.GetByCareerAdvisorIdAsync(request.CareerAdvisorId, cancellationToken);

        if (orphanedCandidates.Count == 0)
        {
            return Result.Success(new ReassignOrphanedCandidatesResponse(0));
        }

        // Deaktive edilen danışman, Host orkestratörünün DeactivateCareerAdvisorCommand'ı Reassign'dan
        // önce çalıştırması sayesinde bu listede zaten yok - ayrıca filtrelemeye gerek yok.
        var activeAdvisorIds = (await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken))
            .Select(a => a.CareerAdvisorId)
            .ToList();

        // Mutable running-tally: aynı batch içindeki adaylar tek bir DB anlık görüntüsüne göre değil,
        // birbiri ardına en-az-yüklü seçimiyle dengeli dağıtılsın diye yerel olarak güncelleniyor.
        var workloadCounts = (await candidateCvRepository.GetCandidateCountsByCareerAdvisorAsync(cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        foreach (var candidateCv in orphanedCandidates)
        {
            var chosenCareerAdvisorId = CareerAdvisorAssignmentSelector.SelectLeastLoaded(activeAdvisorIds, workloadCounts);
            candidateCv.AssignCareerAdvisor(chosenCareerAdvisorId);

            if (chosenCareerAdvisorId is not null)
            {
                workloadCounts[chosenCareerAdvisorId.Value] = workloadCounts.GetValueOrDefault(chosenCareerAdvisorId.Value, 0) + 1;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReassignOrphanedCandidatesResponse(orphanedCandidates.Count));
    }
}
