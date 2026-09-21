using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.RejectCompany;

public sealed class RejectCompanyCommandHandler(
    ICompanyRepository companyRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RejectCompanyCommand, Result>
{
    public async Task<Result> Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        // Reddedilen bir Company, ICompanyRepository.GetCompanyCountsByCareerAdvisorAsync'in
        // Status != Rejected filtresi sayesinde otomatik olarak atanmış danışmanın aktif listesinden
        // düşer (ADR-023) - ayrı bir adım gerekmiyor.
        var rejectResult = company.Reject(request.Reason, DateTime.UtcNow);

        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
