using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;

public sealed class AddCandidateReferenceCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddCandidateReferenceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddCandidateReferenceCommand request, CancellationToken cancellationToken)
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

        var reference = content.AddReference(request.ReferenceTypeId, request.ReferenceLanguageId, request.FirstName, request.LastName);
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

        return Result.Success(reference.Id);
    }
}
