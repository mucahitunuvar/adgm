using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateEducation;

public sealed class UpdateEducationCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEducationCommand, Result>
{
    public async Task<Result> Handle(UpdateEducationCommand request, CancellationToken cancellationToken)
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
        var education = content?.Educations.FirstOrDefault(e => e.Id == request.EducationId);

        if (education is null)
        {
            return Result.Failure(Error.NotFound("Education.NotFound", "The specified education could not be found."));
        }

        var completionStatus = Enum.Parse<EducationCompletionStatus>(request.CompletionStatus, ignoreCase: true);

        education.Update(
            request.EducationLevelId,
            request.StartDate,
            completionStatus,
            request.EndDate,
            request.DiplomaGradingSystemId,
            request.DiplomaGrade,
            request.SchoolId,
            request.SchoolNameFreeText,
            request.ProvinceId,
            request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
