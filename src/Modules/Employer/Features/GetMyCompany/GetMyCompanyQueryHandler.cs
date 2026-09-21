using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyCompany;

// Ownership (AGENTS.md §26): bir Employer kullanıcısı yalnızca kendi firmasını görüntüleyebilir -
// GetCandidateCvQueryHandler'daki ownership-check deseniyle aynı. Diğer firmaların
// görüntülenmesi (Admin/CareerAdvisor) ayrı, rol bazlı GetCompanyQuery üzerinden yapılır.
public sealed class GetMyCompanyQueryHandler(ICompanyRepository companyRepository, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMyCompanyQuery, Result<GetCompanyResponse>>
{
    public async Task<Result<GetCompanyResponse>> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<GetCompanyResponse>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        return Result.Success(GetCompanyResponse.FromDomain(company));
    }
}
