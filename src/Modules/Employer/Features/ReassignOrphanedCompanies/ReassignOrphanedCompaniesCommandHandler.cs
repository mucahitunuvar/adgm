using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.ReassignOrphanedCompanies;

// ReassignOrphanedCandidatesCommandHandler'ın (Candidate modülü) birebir Company karşılığı.
public sealed class ReassignOrphanedCompaniesCommandHandler(
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICompanyRepository companyRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReassignOrphanedCompaniesCommand, Result<ReassignOrphanedCompaniesResponse>>
{
    public async Task<Result<ReassignOrphanedCompaniesResponse>> Handle(
        ReassignOrphanedCompaniesCommand request, CancellationToken cancellationToken)
    {
        var orphanedCompanies = await companyRepository.GetByCareerAdvisorIdAsync(request.CareerAdvisorId, cancellationToken);

        if (orphanedCompanies.Count == 0)
        {
            return Result.Success(new ReassignOrphanedCompaniesResponse(0));
        }

        // Deaktive edilen danışman, Host orkestratörünün DeactivateCareerAdvisorCommand'ı Reassign'dan
        // önce çalıştırması sayesinde bu listede zaten yok - ayrıca filtrelemeye gerek yok.
        var activeAdvisorIds = (await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken))
            .Select(a => a.CareerAdvisorId)
            .ToList();

        // Mutable running-tally: aynı batch içindeki firmalar tek bir DB anlık görüntüsüne göre değil,
        // birbiri ardına en-az-yüklü seçimiyle dengeli dağıtılsın diye yerel olarak güncelleniyor.
        var workloadCounts = (await companyRepository.GetCompanyCountsByCareerAdvisorAsync(cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        foreach (var company in orphanedCompanies)
        {
            var chosenCareerAdvisorId = CareerAdvisorAssignmentSelector.SelectLeastLoaded(activeAdvisorIds, workloadCounts);
            company.AssignCareerAdvisor(chosenCareerAdvisorId);

            if (chosenCareerAdvisorId is not null)
            {
                workloadCounts[chosenCareerAdvisorId.Value] = workloadCounts.GetValueOrDefault(chosenCareerAdvisorId.Value, 0) + 1;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReassignOrphanedCompaniesResponse(orphanedCompanies.Count));
    }
}
