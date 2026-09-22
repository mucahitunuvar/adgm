using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsEmployer;

public sealed class GetMyInterviewsAsEmployerQueryHandler(
    IInterviewRepository interviewRepository, ICompanyModuleContract companyModuleContract, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMyInterviewsAsEmployerQuery, Result<IReadOnlyList<InterviewResponse>>>
{
    public async Task<Result<IReadOnlyList<InterviewResponse>>> Handle(
        GetMyInterviewsAsEmployerQuery request, CancellationToken cancellationToken)
    {
        var company = await companyModuleContract.GetCompanyByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<IReadOnlyList<InterviewResponse>>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var interviews = await interviewRepository.GetByCompanyIdAsync(company.Id, cancellationToken);

        IReadOnlyList<InterviewResponse> response = interviews.Select(InterviewResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
