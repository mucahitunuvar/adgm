using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;

public sealed class CreateLegalDocumentDraftCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLegalDocumentDraftCommand, Result<CreateLegalDocumentDraftResponse>>
{
    public async Task<Result<CreateLegalDocumentDraftResponse>> Handle(
        CreateLegalDocumentDraftCommand request, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document is null)
        {
            return Result.Failure<CreateLegalDocumentDraftResponse>(
                Error.NotFound("LegalDocument.NotFound", $"Legal document '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(document.RowVersion))
        {
            return Result.Failure<CreateLegalDocumentDraftResponse>(Error.Conflict(
                "LegalDocument.ConcurrencyConflict", "The legal document was changed by someone else. Reload and try again."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var createResult = document.CreateDraft(request.ChangeSummary, currentUserContext.UserId!.Value, now);
        if (createResult.IsFailure)
        {
            return Result.Failure<CreateLegalDocumentDraftResponse>(createResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        var draft = document.Versions.First(v => v.Status == LegalDocumentVersionStatus.Draft);
        return Result.Success(new CreateLegalDocumentDraftResponse(draft.Id, draft.VersionNumber));
    }
}
