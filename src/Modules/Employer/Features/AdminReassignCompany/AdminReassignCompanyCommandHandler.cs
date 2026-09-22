using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReassignCompany;

// AdminReassignCandidateCommandHandler'ın (Candidate modülü) birebir Company karşılığı.
public sealed class AdminReassignCompanyCommandHandler(
    ICompanyRepository companyRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminReassignCompanyCommand, Result>
{
    public async Task<Result> Handle(AdminReassignCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
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

        company.AssignCareerAdvisor(request.NewCareerAdvisorId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
