using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetOwnPoolPersonnelNeeds;

// GetJobsPendingReviewQueryHandler'daki gerekçeyle aynı: çağıranın kendi CareerAdvisorId'si
// ICareerAdvisorModuleContract üzerinden çözülür (Identity.User.Id, CareerAdvisor.Id ile aynı değil).
public sealed class GetOwnPoolPersonnelNeedsQueryHandler(
    IPersonnelNeedRepository personnelNeedRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetOwnPoolPersonnelNeedsQuery, Result<IReadOnlyList<PersonnelNeedResponse>>>
{
    public async Task<Result<IReadOnlyList<PersonnelNeedResponse>>> Handle(
        GetOwnPoolPersonnelNeedsQuery request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<IReadOnlyList<PersonnelNeedResponse>>(
                Error.Forbidden("PersonnelNeed.NotACareerAdvisor", "Only an active career advisor may view their own pool."));
        }

        var personnelNeeds = await personnelNeedRepository.GetOwnPoolByAdvisorIdAsync(callerAdvisorId.Value, cancellationToken);

        IReadOnlyList<PersonnelNeedResponse> response = personnelNeeds.Select(PersonnelNeedResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
