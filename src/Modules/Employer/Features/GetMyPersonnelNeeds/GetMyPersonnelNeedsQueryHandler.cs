using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;

// Ownership: CompanyId request'ten değil, ICompanyRepository.GetByUserIdAsync(currentUserContext.UserId)
// zincirinden çözülür (GetMyCompanyJobsQueryHandler deseni).
public sealed class GetMyPersonnelNeedsQueryHandler(
    ICompanyRepository companyRepository, IPersonnelNeedRepository personnelNeedRepository, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMyPersonnelNeedsQuery, Result<IReadOnlyList<PersonnelNeedResponse>>>
{
    public async Task<Result<IReadOnlyList<PersonnelNeedResponse>>> Handle(
        GetMyPersonnelNeedsQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<IReadOnlyList<PersonnelNeedResponse>>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var personnelNeeds = await personnelNeedRepository.GetByCompanyIdAsync(company.Id, cancellationToken);

        IReadOnlyList<PersonnelNeedResponse> response = personnelNeeds.Select(PersonnelNeedResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
