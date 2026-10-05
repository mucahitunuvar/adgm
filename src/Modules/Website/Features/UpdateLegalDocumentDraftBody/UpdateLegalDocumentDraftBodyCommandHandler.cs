using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

public sealed class UpdateLegalDocumentDraftBodyCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLegalDocumentDraftBodyCommand, Result>
{
    public async Task<Result> Handle(UpdateLegalDocumentDraftBodyCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var sanitizedBody = htmlContentSanitizer.Sanitize(request.Body ?? string.Empty);

        var updateResult = document.UpdateDraftBody(
            languageCodeResult.Value, sanitizedBody, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
