using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateLanguage;

public sealed class AddCandidateLanguageCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddCandidateLanguageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddCandidateLanguageCommand request, CancellationToken cancellationToken)
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

        var language = content.AddLanguage(request.LanguageId, request.LanguageLevelId, request.IsNativeLanguage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(language.Id);
    }
}
