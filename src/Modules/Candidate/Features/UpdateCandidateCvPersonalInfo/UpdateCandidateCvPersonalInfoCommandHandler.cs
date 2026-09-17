using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

public sealed class UpdateCandidateCvPersonalInfoCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCandidateCvPersonalInfoCommand, Result>
{
    public async Task<Result> Handle(UpdateCandidateCvPersonalInfoCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV."));
        }

        candidateCv.UpdatePersonalInfo(
            request.Title,
            request.GenderId,
            request.BirthDate,
            request.DriversLicenseTypeId,
            request.NationalityId,
            request.NetSalaryExpectation,
            request.MilitaryStatusId);

        var disabilityInfo = request.DisabilityInfo is null
            ? null
            : DisabilityInfo.Create(
                request.DisabilityInfo.CategoryId,
                request.DisabilityInfo.Percentage,
                request.DisabilityInfo.Description,
                request.DisabilityInfo.HasHealthReport,
                request.DisabilityInfo.UsesMedication,
                request.DisabilityInfo.HasChronicCondition,
                request.DisabilityInfo.HasContagiousDisease,
                request.DisabilityInfo.HasConsciousnessLossRisk);

        candidateCv.UpdateDisabilityInfo(disabilityInfo);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
