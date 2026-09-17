using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.AddExperience;

public sealed class AddExperienceCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddExperienceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddExperienceCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<Guid>(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<Guid>(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV content."));
        }

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (content is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("CandidateCvContent.NotFound", "The specified candidate CV content could not be found."));
        }

        var experience = content.AddExperience(request.CompanyName, request.StartDate);
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

        return Result.Success(experience.Id);
    }
}
