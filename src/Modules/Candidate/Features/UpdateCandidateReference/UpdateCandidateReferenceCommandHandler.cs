using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateReference;

public sealed class UpdateCandidateReferenceCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCandidateReferenceCommand, Result>
{
    public async Task<Result> Handle(UpdateCandidateReferenceCommand request, CancellationToken cancellationToken)
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
        var reference = content?.References.FirstOrDefault(r => r.Id == request.CandidateReferenceId);

        if (reference is null)
        {
            return Result.Failure(Error.NotFound("CandidateReference.NotFound", "The specified reference could not be found."));
        }

        reference.Update(
            request.ReferenceTypeId,
            request.ReferenceLanguageId,
            request.FirstName,
            request.LastName,
            request.Company,
            request.Position,
            request.Email,
            request.PhoneNumber);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
