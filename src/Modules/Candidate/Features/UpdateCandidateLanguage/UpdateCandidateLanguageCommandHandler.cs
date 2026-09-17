using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateLanguage;

public sealed class UpdateCandidateLanguageCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCandidateLanguageCommand, Result>
{
    public async Task<Result> Handle(UpdateCandidateLanguageCommand request, CancellationToken cancellationToken)
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
        var language = content?.Languages.FirstOrDefault(l => l.Id == request.CandidateLanguageId);

        if (language is null)
        {
            return Result.Failure(Error.NotFound("CandidateLanguage.NotFound", "The specified language could not be found."));
        }

        language.Update(request.LanguageId, request.LanguageLevelId, request.IsNativeLanguage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
