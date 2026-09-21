using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetCompany;

// Admin/CareerAdvisor tarafı için - herhangi bir firmayı id ile görüntüleme (rol bazlı gate,
// SearchCandidates deseniyle aynı). Kendi firmasını görüntüleyen Employer kullanıcısı için
// GetMyCompanyQuery ayrı bir feature (GetCandidateCv'nin ownership-check deseni).
public sealed class GetCompanyQueryHandler(ICompanyRepository companyRepository)
    : IRequestHandler<GetCompanyQuery, Result<GetCompanyResponse>>
{
    public async Task<Result<GetCompanyResponse>> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure<GetCompanyResponse>(
                Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        return Result.Success(GetCompanyResponse.FromDomain(company));
    }
}
