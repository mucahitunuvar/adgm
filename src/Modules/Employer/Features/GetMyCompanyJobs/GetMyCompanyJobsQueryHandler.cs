using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyCompanyJobs;

// Ownership: CompanyId request'ten değil, ICompanyRepository.GetByUserIdAsync(currentUserContext.UserId)
// zincirinden çözülür (GetMyCompanyQueryHandler deseni).
public sealed class GetMyCompanyJobsQueryHandler(
    ICompanyRepository companyRepository, IJobRepository jobRepository, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMyCompanyJobsQuery, Result<IReadOnlyList<JobResponse>>>
{
    public async Task<Result<IReadOnlyList<JobResponse>>> Handle(
        GetMyCompanyJobsQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<IReadOnlyList<JobResponse>>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var jobs = await jobRepository.GetByCompanyIdAsync(company.Id, cancellationToken);

        IReadOnlyList<JobResponse> response = jobs.Select(JobResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
