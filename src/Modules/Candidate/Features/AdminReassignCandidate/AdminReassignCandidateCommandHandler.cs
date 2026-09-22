using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.AdminReassignCandidate;

// PROJECT.md §5.3: admin, otomatik en-az-yük atamasından bağımsız olarak belirli bir adayı elle
// seçtiği danışmana atayabilmeli (ReassignOrphanedCandidatesCommand'ın aksine, burada danışman
// seçimi algoritmik değil, admin'in kendi kararı - yalnızca var/aktif olduğu doğrulanır).
public sealed class AdminReassignCandidateCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminReassignCandidateCommand, Result>
{
    public async Task<Result> Handle(AdminReassignCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (request.NewCareerAdvisorId is not null)
        {
            var activeAdvisors = await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken);
            var isActiveAdvisor = activeAdvisors.Any(a => a.CareerAdvisorId == request.NewCareerAdvisorId);

            if (!isActiveAdvisor)
            {
                return Result.Failure(Error.NotFound(
                    "CareerAdvisor.NotFound", "The specified career advisor could not be found or is not active."));
            }
        }

        candidateCv.AssignCareerAdvisor(request.NewCareerAdvisorId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
