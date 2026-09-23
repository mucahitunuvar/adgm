using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;

// CreateCandidateSuggestionCommandHandler'daki "yalnızca kendi adayını önerebilir" kontrolünün
// birebir aynısı - burada "yalnızca kendi adayı için SkillGap tanımlayabilir".
public sealed class CreateSkillGapCommandHandler(
    ISkillGapRepository skillGapRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSkillGapCommand, Result<CreateSkillGapResponse>>
{
    public async Task<Result<CreateSkillGapResponse>> Handle(CreateSkillGapCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateSkillGapResponse>(
                Error.Forbidden("SkillGap.NotACareerAdvisor", "Only an active career advisor may identify a skill gap."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateSkillGapResponse>(
                Error.Forbidden("SkillGap.NotOwnCandidate", "You may only identify skill gaps for your own candidates."));
        }

        var skillGap = SkillGap.Create(request.CandidateCvId, request.SkillId, callerAdvisorId.Value, request.Notes, DateTime.UtcNow);

        skillGapRepository.Add(skillGap);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateSkillGapResponse(skillGap.Id));
    }
}
