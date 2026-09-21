using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveCompany;

public sealed class ApproveCompanyCommandHandler(
    ICompanyRepository companyRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveCompanyCommand, Result>
{
    public async Task<Result> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        // approvedByUserId ICurrentUserContext'ten alınır, request gövdesinden değil (AGENTS.md §26 -
        // client-taraflı bir kullanıcı id'sine asla güvenilmez).
        var approveResult = company.Approve(currentUserContext.UserId!.Value, DateTime.UtcNow);

        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
