using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;

public sealed class DeactivateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateCompanyCommand, Result>
{
    public async Task<Result> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        // deactivatedByUserId ICurrentUserContext'ten alınır, request gövdesinden değil (AGENTS.md §26).
        // Deaktive edilen bir Company, ICompanyRepository.GetCompanyCountsByCareerAdvisorAsync'in
        // Status != Deactivated filtresi sayesinde otomatik olarak atanmış danışmanın aktif listesinden
        // düşer (ADR-023); geçmiş kaydı korunur, yalnızca Status değişir.
        var deactivateResult = company.Deactivate(currentUserContext.UserId!.Value, DateTime.UtcNow);

        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
