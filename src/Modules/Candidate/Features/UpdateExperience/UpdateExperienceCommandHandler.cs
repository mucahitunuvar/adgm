using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;

public sealed class UpdateExperienceCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateExperienceCommand, Result>
{
    public async Task<Result> Handle(UpdateExperienceCommand request, CancellationToken cancellationToken)
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
        var experience = content?.Experiences.FirstOrDefault(e => e.Id == request.ExperienceId);

        if (experience is null)
        {
            return Result.Failure(Error.NotFound("Experience.NotFound", "The specified experience could not be found."));
        }

        experience.Update(
            request.CompanyName,
            request.PositionId,
            request.StartDate,
            request.EndDate,
            request.IsCurrentJob,
            request.SectorId,
            request.WorkFieldId,
            request.EmploymentTypeId,
            request.CountryId,
            request.ProvinceId,
            request.JobDescription);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
