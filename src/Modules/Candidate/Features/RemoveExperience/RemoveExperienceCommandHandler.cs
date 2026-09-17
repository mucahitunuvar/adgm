using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveExperience;

public sealed class RemoveExperienceCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveExperienceCommand, Result>
{
    public async Task<Result> Handle(RemoveExperienceCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV content."));
        }

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (content is null)
        {
            return Result.Failure(
                Error.NotFound("CandidateCvContent.NotFound", "The specified candidate CV content could not be found."));
        }

        content.RemoveExperience(request.ExperienceId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
