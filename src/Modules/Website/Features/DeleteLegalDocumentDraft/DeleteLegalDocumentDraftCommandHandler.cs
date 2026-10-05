using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocumentDraft;

public sealed class DeleteLegalDocumentDraftCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLegalDocumentDraftCommand, Result>
{
    public async Task<Result> Handle(DeleteLegalDocumentDraftCommand request, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.NotFound", $"Legal document '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(document.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "LegalDocument.ConcurrencyConflict", "The legal document was changed by someone else. Reload and try again."));
        }

        var deleteResult = document.DeleteDraft(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
